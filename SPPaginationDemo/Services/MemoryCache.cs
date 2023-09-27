using SPPaginationDemo.CallLogger;
using System.Collections.Concurrent;

// ReSharper disable InconsistentlySynchronizedField

namespace SPPaginationDemo.Services;

public static class MemoryCache
{
    //Todo: DS: Replace MemoryCache class with Generator that creates getters with backing fields
    private static readonly ConcurrentDictionary<string, object> Cache = new();

    [Log]
    public static T LazyLoadAndCache<T>(string key, Func<T> valueFactory)
        => (T)Cache.GetOrAdd(key, new Lazy<object>(() => valueFactory()!, LazyThreadSafetyMode.ExecutionAndPublication));

    [Log]
    public static T LazyLoadAndCache<T>(string key, Func<T> valueFactory, out bool fromMemory)
    {
        fromMemory = Cache.ContainsKey(key);

        return (T)Cache.GetOrAdd(key, new Lazy<object>(() => valueFactory()!, LazyThreadSafetyMode.ExecutionAndPublication));
    }
}