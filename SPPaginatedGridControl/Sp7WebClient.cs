using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using DevExpress.XtraEditors;
using Microsoft.AspNetCore.Mvc;
using SPUpdateFramework;
using SPUpdateFramework.Extensions;

namespace SPPaginatedGridControl;

public class Sp7WebClient
{
    private HttpClient? _client;

    private HttpClient Client
    {
        get
        {
            if (_client != null) return _client;

            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7269/")
            };

            // Add default headers as necessary
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "Sp7DemoUpdate");

            return _client;
        }
    }

    /// <summary>
    /// Execute Callback on Server if release mode and run locally if debug mode
    /// </summary>
    /// <typeparam name="TEnpointCallback"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="dto"></param>
    /// <param name="runOnServer">Run on server if Debugger is attached</param>
    /// <returns></returns>
    public async Task<string> CallEnpointAsync<TEnpointCallback, TDto>(TDto dto, bool runOnServer = false)
        where TEnpointCallback : IEndpointCallback where TDto : class
    {
        var json = System.Text.Json.JsonSerializer.Serialize(dto);

        // Check if we are running in debug mode
        if (!runOnServer && Debugger.IsAttached)
        {
            try
            {
                // Run locally
                var endpointCallback = Activator.CreateInstance<TEnpointCallback>();

                var actionResult = endpointCallback.OnEndpointCallback(json);

                if (actionResult is JsonResult jsonResult)
                    return jsonResult.Value?.ToString() ?? string.Empty;
            }
            catch (NotImplementedException)
            {
                return "Callback not implemented in blueprint assembly!";
            }
        }

        // Run on server
        var assemblyName = typeof(TEnpointCallback).Assembly.GetName().Name;
        var callbackName = typeof(TEnpointCallback).FullName?.Split('.').Last();
        for (var i = 0; ; i++)
        {
            // Try to run method on server and if it returns 404, then upload the assembly and try again
            // Enpoint live-update/callback/{AssemblyName}/{CallbackTypeName}
            var response = await Client.PostAsync($"live-update/callback/{assemblyName}/{callbackName}", new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadAsStringAsync()).Replace("\"", "");

            if (i > 0)
                throw new Exception("Failed to run callback on server");

            // Upload the assembly to the server
            var callbackAssembly = typeof(TEnpointCallback).Assembly;
            var assemblyBytes = await File.ReadAllBytesAsync(callbackAssembly.Location);
            var assemblyBytesCompressed = assemblyBytes.Compress();

            var publicKey = (await Client.GetStringAsync("/live-update/get-public-key")).Replace("\"", "");

            var publicKeyString = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey));
            var publicKeyRsa = RSA.Create();
            publicKeyRsa.ImportFromPem(publicKeyString);

            var encryptedAssemblyBytes = assemblyBytesCompressed.HybridEncrypt(publicKeyRsa);

            var encryptedAssemblyString = Convert.ToBase64String(encryptedAssemblyBytes);

            var uploadResponse = await Client.PostAsync("/live-update/update-endpoint", new StringContent(encryptedAssemblyString));

            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception("Failed to upload assembly");
        }
    }
}