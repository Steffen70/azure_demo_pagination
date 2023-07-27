using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using SPPaginationDemo.Extensions;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7269/")
};

httpClient.DefaultRequestHeaders.Accept.Clear();
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
httpClient.DefaultRequestHeaders.Add("User-Agent", "DemoUpdateClient");

var publicKey = (await httpClient.GetStringAsync("/live-update/get-public-key")).Replace("\"", "");

var publicKeyString = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey));

var publicRsa = RSA.Create();
publicRsa.ImportFromPem(publicKeyString);

var assemblyPath = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "DemoUpdate", "bin", "Debug", "net7.0-windows", "DemoUpdate.dll"));

var assemblyBytes = await File.ReadAllBytesAsync(assemblyPath.FullName);

var compressedAssemblyBytes = assemblyBytes.Compress();

var encryptedAssemblyBytes = compressedAssemblyBytes.HybridEncrypt(publicRsa);

var encryptedAssemblyString = Convert.ToBase64String(encryptedAssemblyBytes);

var response = await httpClient.PostAsync("/live-update/update-endpoint", new StringContent(encryptedAssemblyString));

var assemblyName = await response.Content.ReadAsStringAsync();

Console.WriteLine($"Assembly name: {assemblyName}");