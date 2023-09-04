using System.Reflection;
using SPPaginationDemo.CallLogger;
using SPPaginationDemo.Extensions;
using SPPaginationDemo.Services;

#pragma warning disable CA2254

namespace SPPaginationDemo.DtoGenerator;

public abstract class BaseFactory
{
    protected ILogger Logger { get; }

    public abstract string AssemblyString { get; }

    public abstract string SqlIdentifier { get; }

    [Log]
    public string TypeName => $"DynamicType_{SqlIdentifier}";

    [Log]
    public byte[] AssemblyBytes =>
        MemoryCache.LazyLoadAndCache($"{TypeName}_AssemblyBytes", () => Convert.FromBase64String(AssemblyString).Decompress());

    [Log]
    public Type Model
    {
        get
        {
            var model = MemoryCache.LazyLoadAndCache($"{TypeName}_Model", () =>
            {
                var assembly = Assembly.Load(AssemblyBytes);

                return assembly.GetTypes().First(t => t.Name == TypeName);
            }, out var fromMemory);

            Logger.LogInformation($"Retrieved model from {(fromMemory ? "memory" : "redis")} cache for '{SqlIdentifier}'.");

            return model;
        }
    }

    protected BaseFactory(ILogger logger)
    {
        Logger = logger;
    }
}