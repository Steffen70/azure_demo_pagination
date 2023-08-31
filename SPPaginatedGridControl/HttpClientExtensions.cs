using System.Diagnostics;
using System.Reflection;
using SPPaginationDemo.Extensions;

namespace SPPaginatedGridControl;

public static class HttpClientExtensions
{
    private static readonly Dictionary<string, Type> TypeCache = new();

    public static async Task<Type> LoadTypeFromServerOrCacheAsync(this HttpClient client, string actionName, Stopwatch? stopwatch = null)
    {
        var id = (await client.GetStringAsync($"/Paginated/sql-query-identifier/{actionName}")).Replace("\"", "");

        stopwatch?.Start();

        if (TypeCache.TryGetValue(id, out var cachedDto))
        {
            stopwatch?.Stop();
            return cachedDto;
        }

        var dtoType = await LoadTypeFromCacheAsync(id) ?? await LoadTypeFromServerAsync(client, id);

        stopwatch?.Stop();

        TypeCache.Add(id, dtoType);
        return dtoType;
    }

    private static async Task<Type> LoadTypeFromServerAsync(HttpClient client, string id)
    {
        Debug.WriteLine(@"Loading DTO From Server");

        // Get the assembly bytes from the REST API
        var assemblyString = (await client.GetStringAsync($"/Paginated/assembly-bytes/{id}")).Replace("\"", "");

        var assemblyBytes = Convert.FromBase64String(assemblyString).Decompress();

        // Load the assembly and get the type
        var assembly = await Task.Run(() => Assembly.Load(assemblyBytes));
        var typeName = $"DynamicType_{id}";
        var modelType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName) 
                        ?? throw new InvalidOperationException("The model type is not set.");

        await CacheTypeAsync(assemblyBytes, id);

        return modelType;
    }

    private static async Task CacheTypeAsync(byte[] assemblyBytes, string id)
    {
        // Create folder TypeCache if it does not exist
        Directory.CreateDirectory("TypeCache");

        // Save assembly to file
        var path = Path.Combine("TypeCache", $"{id}.dll");
        await File.WriteAllBytesAsync(path, assemblyBytes);
    }

    private static async Task<Type?> LoadTypeFromCacheAsync(string id)
    {
        // Check if Fileexists id + ".dll" in folder TypeCache
        var path = Path.Combine("TypeCache", $"{id}.dll");
        if (!File.Exists(path)) return null;

        // Load assembly asynchonously
        var assemblyBytes = await File.ReadAllBytesAsync(path);

        // Load assemly in separate task to avoid blocking the UI thread
        var assembly = await Task.Run(() => Assembly.Load(assemblyBytes));

        var typeName = $"DynamicType_{id}";
        var modelType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName)
                        ?? throw new InvalidOperationException("The model type is not set.");

        Debug.WriteLine(@"Loaded DTO From RedisCacheFactory");

        return modelType;
    }
}