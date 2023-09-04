using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using SPPaginationDemo.CallLogger;
using SPPaginationDemo.Services;
using StackExchange.Redis;

#pragma warning disable CA2254

namespace SPPaginationDemo.Extensions;

public static class RsaExtensions
{
    [Log]
    public static RSA ImportKeyAndCache(this RSA rsa, string keyPath, IDatabase? redisDatabase = null, ILogger? logger = null)
    {
        var keyName = Path.GetFileNameWithoutExtension(keyPath);

        if (redisDatabase != null && redisDatabase.KeyExists(keyName))
        {
            var value = MemoryCache.LazyLoadAndCache(keyName, () => redisDatabase.StringGet(keyName).ToString(), out var fromMemory);
            rsa.FromXmlString(value!);

            logger?.LogInformation(!fromMemory
                ? $"RSA key '{keyName}' loaded from Redis cache."
                : $"RSA key '{keyName}' loaded from memory cache.");

            return rsa;
        }

        using var reader = File.OpenText(keyPath);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        var rsaParameters = keyObject switch
        {
            RsaPrivateCrtKeyParameters privateKey => new RSAParameters
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned()
            },
            RsaKeyParameters publicKey => new RSAParameters
            {
                Modulus = publicKey.Modulus.ToByteArrayUnsigned(),
                Exponent = publicKey.Exponent.ToByteArrayUnsigned()
            },
            _ => throw new InvalidOperationException("Invalid key format.")
        };

        rsa.ImportParameters(rsaParameters);

        if (redisDatabase != null)
        {
            var xmlString = rsa.ToXmlString(true);
            redisDatabase.StringSet(keyName, xmlString);
            MemoryCache.LazyLoadAndCache(keyName, () => xmlString);

            logger?.LogInformation($"RSA key '{keyName}' loaded from file and cached in Redis.");
        }
        else
            logger?.LogInformation($"RSA key '{keyName}' loaded from file.");

        return rsa;
    }
}