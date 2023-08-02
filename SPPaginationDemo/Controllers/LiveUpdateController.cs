using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static SPPaginationDemo.ModelGenerator.DynamicTypeGenerator;
using System.Security.Cryptography;
using System.Text;
using SPUpdateFramework;
using SPUpdateFramework.Extensions;

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("live-update/")]
public class LiveUpdateController : Controller
{
    private const string AssembliesFolder = "UpdateAsseblies";

    internal class AssemblyRegistration
    {
        public string AssemblyName { get; set; } = null!;
        public string Identifier { get; set; } = null!;
        public Assembly Assembly { get; set; } = null!;

        public Dictionary<string, IEndpointCallback> Callbacks { get; } = new();
    }

    private static List<AssemblyRegistration> UpdateAssemblies { get; } = new();

    [HttpPost("callback/{assemblyName}/{callback}")]
    public async Task<ActionResult> Callback(string assemblyName, string callback)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);

        var json = await reader.ReadToEndAsync();

        var registration = UpdateAssemblies.FirstOrDefault(r => r.AssemblyName == assemblyName);
        if (registration == null)
            return NotFound();

        var callbackType = registration.Assembly.GetTypes().FirstOrDefault(t => t.Name == callback);
        if (callbackType == null)
            return NotFound();

        if ((registration.Callbacks.FirstOrDefault(c => c.Key == callback).Value ?? Activator.CreateInstance(callbackType)) is not IEndpointCallback callbackInstance)
            return NotFound();

        try
        {
            registration.Callbacks.Add(callback, callbackInstance);
        }
        catch (Exception)
        {
            // ignored
        }

        var callbackTask = new Task<ActionResult>(() => callbackInstance.OnEndpointCallback(json));

        callbackTask.Start();

        return await callbackTask;
    }

    [HttpPost("update-endpoint/")]
    public async Task<ActionResult<string>> AddUpdate()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);

        var assemblyString = await reader.ReadToEndAsync();

        var identifier = GetIdentifier(assemblyString);

        if (UpdateAssemblies.Any(r => r.Identifier == identifier))
            return new StatusCodeResult((int)HttpStatusCode.AlreadyReported);

        var assemblyBytesEncrypted = Convert.FromBase64String(assemblyString);

        var privateKey = await System.IO.File.ReadAllTextAsync(Path.Combine("EncryptionKeys", "private_key.pem"));

        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(privateKey);

        var assemblyBytes = assemblyBytesEncrypted.HybridDecrypt(privateRsa);

        var assemblyDecompressed = assemblyBytes.Decompress();

        var assembly = Assembly.Load(assemblyDecompressed);

        if (!Directory.Exists(AssembliesFolder))
            Directory.CreateDirectory(AssembliesFolder);

        var guid = Guid.NewGuid();
        await System.IO.File.WriteAllBytesAsync(Path.Combine(AssembliesFolder, $"assembly_{guid}.dll"), assemblyDecompressed);

        var assemblyName = assembly.GetName().Name!;

        System.IO.File.Move(Path.Combine(AssembliesFolder, $"assembly_{guid}.dll"), Path.Combine(AssembliesFolder, $"{assemblyName}.dll"));

        System.IO.File.Delete(Path.Combine(AssembliesFolder, $"assembly_{guid}.dll"));

        UpdateAssemblies.Add(new AssemblyRegistration { Assembly = assembly, AssemblyName = assemblyName, Identifier = identifier });

        return Ok(assemblyName);
    }

    [HttpDelete("delete-update/{assemblyName}")]
    public ActionResult DeleteUpdate(string assemblyName)
    {
        var registration = UpdateAssemblies.FirstOrDefault(r => r.AssemblyName == assemblyName);
        if (registration == null)
            return NotFound();

        var assemblyFile = Directory.GetFiles(AssembliesFolder).Select(Path.GetFileName).FirstOrDefault(n => n == $"{assemblyName}.dll");

        if (assemblyFile == null)
            return NotFound();

        System.IO.File.Delete(assemblyFile);

        UpdateAssemblies.Remove(registration);

        return Ok();
    }

    internal static async Task LoadAllUpdateAssemblies()
    {
        if (!Directory.Exists(AssembliesFolder))
            return;

        var assemblyFiles = Directory.GetFiles(AssembliesFolder);

        foreach (var assemblyFile in assemblyFiles)
        {
            var assemblyBytes = await System.IO.File.ReadAllBytesAsync(assemblyFile);

            var assembly = Assembly.Load(assemblyBytes);

            var assemblyString = Convert.ToBase64String(assemblyBytes);

            var identifier = GetIdentifier(assemblyString);

            var assemblyName = assembly.GetName().Name!;

            var registration = new AssemblyRegistration { Assembly = assembly, AssemblyName = assemblyName, Identifier = identifier };

            UpdateAssemblies.Add(registration);
        }
    }

    [HttpGet("get-public-key/")]
    public async Task<string> GetPublicKeyBase64()
    {
        var publicKey = await System.IO.File.ReadAllTextAsync(Path.Combine("EncryptionKeys", "public_key.pem"));
        var encryptedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKey));

        return encryptedKey;
    }
}