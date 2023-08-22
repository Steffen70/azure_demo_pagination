using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace SPPaginationDemo.Extensions;

public static class RsaExtensions
{
    private static readonly Dictionary<string, RSA> RsaCache = new();

    public static RSA ImportKeyAndCache(this RSA rsa, string keyPath)
    {
        if (RsaCache.TryGetValue(keyPath, out var key))
            return key;

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

        RsaCache.Add(keyPath, rsa);

        return rsa;
    }
}