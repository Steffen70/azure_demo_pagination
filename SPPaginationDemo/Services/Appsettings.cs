using Microsoft.Extensions.Primitives;
using SPPaginationDemo.CallLogger;
using SPPaginationDemo.Extensions;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable IDE0290

namespace SPPaginationDemo.Services;

public class Appsettings : IConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public string ContentRootPath { get; }

    [Log]
    private ILogger Logger => MemoryCache.LazyLoadAndCache("Logger", _serviceProvider.GetRequiredService<ILogger>);
    [Log]
    public IDatabase RedisDatabase => MemoryCache.LazyLoadAndCache("RedisDatabase", () => ConnectionMultiplexer.Connect(RedisConnectionString).GetDatabase());

    private const string ConnectionStringExceptionMessage = "No connection string named '{0}' was found in the configuration.";
    [Log]
    public string RedisConnectionString => MemoryCache.LazyLoadAndCache("RedisConnectionString", () => GetConnectionString("Redis", "RedisPrimary", ConnectionStringExceptionMessage, false));
    [Log]
    public string SqlConnectionString => MemoryCache.LazyLoadAndCache("SqlConnectionString", () => GetConnectionString("Sql", "Sql", ConnectionStringExceptionMessage));

    [Log]
    public Appsettings(IConfiguration configuration, IHostEnvironment env, IServiceProvider serviceProvider)
    {
        ContentRootPath = env.ContentRootPath;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    [Log]
    public string GetDecryptedPassword(string keyIdentifier, bool cacheOnRedis = true)
    {
        var encryptedPasswordBase64 = _configuration[$"EncryptedPasswords:{keyIdentifier}"];

        if (string.IsNullOrEmpty(encryptedPasswordBase64))
            throw new InvalidOperationException("No parameter named 'EncryptedPassword' was found in the configuration.");

        var encryptedPasswordBytes = Convert.FromBase64String(encryptedPasswordBase64);

        var rsa = RSA.Create().ImportKeyAndCache(Path.Combine(ContentRootPath, "EncryptionKeys", "private_key.pem"), cacheOnRedis ? RedisDatabase : null, cacheOnRedis ? Logger : null);

        // Decrypt the password
        var decryptedPassword = encryptedPasswordBytes.HybridDecrypt(rsa);
        var passwordString = Encoding.UTF8.GetString(decryptedPassword);
        return passwordString;
    }

    [Log]
    private string GetConnectionString(string name, string passwordKey, string exceptionMessage, bool cacheOnRedis = true)
    {
        var connString = _configuration.GetConnectionString(name) ??
                         throw new InvalidOperationException(string.Format(exceptionMessage, name));
        var password = GetDecryptedPassword(passwordKey, cacheOnRedis);
        return string.Format(connString, password);
    }

    [Log]
    public string? this[string key]
    {
        get => _configuration[key];
        set => _configuration[key] = value;
    }

    [Log]
    public IEnumerable<IConfigurationSection> GetChildren() => _configuration.GetChildren();

    [Log]
    public IChangeToken GetReloadToken() => _configuration.GetReloadToken();

    [Log]
    public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
}
