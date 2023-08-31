namespace SPPaginationDemo.Services;

public static class MemoryCache
{
    private static readonly Dictionary<string, object> Cache = new();

    public static T LazyLoadAndCache<T>(string key, Func<T> valueFactory) => LazyLoadAndCache(key, valueFactory, out _);

    public static T LazyLoadAndCache<T>(string key, Func<T> valueFactory, out bool fromMemory)
    {
        lock (Cache) 
        {
            if (Cache.TryGetValue(key, out var value))
            {
                fromMemory = true;
                return (T)value;
            }

            var newValue = valueFactory();
            Cache[key] = newValue!;

            fromMemory = false;
            return newValue;
        }
    }
}