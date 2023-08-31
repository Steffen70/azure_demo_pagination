using SPPaginationDemo.Services;
using StackExchange.Redis;

#pragma warning disable CA2254

namespace SPPaginationDemo.DtoGenerator;

public class RedisCacheFactory : BaseFactory
{
    private readonly Appsettings _appsettings;

    private IDatabase RedisDatabase => _appsettings.RedisDatabase;

    private string RedisGeneratorLockKey => $"{SqlIdentifier}_Lock";

    public sealed override string SqlIdentifier { get; }

    public bool IsCached => RedisDatabase.KeyExists(SqlIdentifier);

    public override string AssemblyString
    {
        get
        {
            var assemblyString = MemoryCache.LazyLoadAndCache(SqlIdentifier, () => RedisDatabase.StringGet(SqlIdentifier).ToString(), out var fromMemory);

            Logger.LogInformation(
                $"Retrieved assembly string from {(fromMemory ? "memory" : "redis")} cache for '{SqlIdentifier}'.");

            return assemblyString!;
        }
    }

    public RedisCacheFactory(ILogger logger, Appsettings appsettings, string sqlIdentifier) : base(logger)
    {
        SqlIdentifier = sqlIdentifier;
        _appsettings = appsettings;
    }

    public RedisCacheFactory(ILogger logger, Appsettings appsettings, BaseFactory dtoFactory) : base(logger)
    {
        if (dtoFactory is RedisCacheFactory)
            throw new ArgumentException("Cannot create RedisCacheFactory from another RedisCacheFactory.");

        SqlIdentifier = dtoFactory.SqlIdentifier;
        _appsettings = appsettings;

        // If the type is already in cache, return immediately else wait for the lock to be released or acquire the lock and generate the type
        if (AwaitRedisLock())
            return;

        // Generate type by calling the getter that will generate the type and cache it in memory
        var assemblyString = dtoFactory.AssemblyString;

        // Add assembly string to cache for 1 day
        RedisDatabase.StringSet(SqlIdentifier, assemblyString, TimeSpan.FromDays(1));

        // Release lock
        RedisDatabase.KeyDelete(RedisGeneratorLockKey);

        MemoryCache.LazyLoadAndCache(SqlIdentifier, () => assemblyString);

        Logger.LogInformation($"Added assembly string to cache for '{SqlIdentifier}'.");
    }

    private bool AwaitRedisLock()
    {
        while (true)
        {
            var exists = RedisDatabase.KeyExists(SqlIdentifier);
            var typeGenerationInProgress = RedisDatabase.KeyExists(RedisGeneratorLockKey);

            if (exists || !typeGenerationInProgress)
            {
                // Type exists and type generation is not in progress, return true to indicate that the type is in cache
                if (exists) return true;

                // Get lock and if it fails, wait for 1 second and try again
                // lock redis and set string
                if (!RedisDatabase.StringSet(RedisGeneratorLockKey, true, TimeSpan.FromMinutes(5), When.NotExists))
                    continue;

                // Lock acquired, return false to indicate that the lock was acquired but the type was not in cache
                return false;
            }

            Logger.LogInformation($"Waiting for Redis lock for '{SqlIdentifier}'.");

            Thread.Sleep(5000);
        }
    }
}