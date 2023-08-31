using System.Reflection;
using SPPaginationDemo.Extensions;
using SPPaginationDemo.Services;

namespace SPPaginationDemo.DtoGenerator;

public abstract class BaseFactory
{
    protected ILogger Logger { get; }

    public abstract string AssemblyString { get; }

    public abstract string SqlIdentifier { get; }

    public string TypeName => $"DynamicType_{SqlIdentifier}";

    public byte[] AssemblyBytes =>
        MemoryCache.LazyLoadAndCache($"{TypeName}_AssemblyBytes", () => Convert.FromBase64String(AssemblyString).Decompress());

    public Type Model => MemoryCache.LazyLoadAndCache($"{TypeName}_Model", () =>
    {
        var assembly = Assembly.Load(AssemblyBytes);

        var model = assembly.GetTypes().First(t => t.Name == TypeName);

        return model;
    });

    protected BaseFactory(ILogger logger)
    {
        Logger = logger;
    }
}