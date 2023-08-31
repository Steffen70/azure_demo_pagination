using System.IO.Compression;
using System.Security.Cryptography;

namespace SPPaginationDemo.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] Compress(this byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);

        zipStream.Write(data, 0, data.Length);
        zipStream.Close();
        return compressedStream.ToArray();
    }

    public static byte[] Decompress(this byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();

        var buffer = new byte[4096];
        int read;

        while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0) resultStream.Write(buffer, 0, read);

        return resultStream.ToArray();
    }

    public static byte[] HybridEncrypt(this byte[] data, RSA publicKey)
    {
        // Generate a new AES key
        using var aes = Aes.Create();

        aes.GenerateKey();
        aes.GenerateIV();

        // Encrypt the data with the AES key
        byte[] encryptedData;
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) 
                cs.Write(data, 0, data.Length);

            encryptedData = ms.ToArray();
        }

        // Encrypt the AES key with the RSA public key
        var encryptedKey = publicKey.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

        // Concatenate the AES IV, the encrypted AES key, and the encrypted data into a single array
        var result = new byte[aes.IV.Length + encryptedKey.Length + encryptedData.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedKey, 0, result, aes.IV.Length, encryptedKey.Length);
        Buffer.BlockCopy(encryptedData, 0, result, aes.IV.Length + encryptedKey.Length, encryptedData.Length);

        return result;
    }

    public static byte[] HybridDecrypt(this byte[] encryptedDataWithKey, RSA privateKey)
    {
        // Extract the AES IV, the encrypted AES key, and the encrypted data from the input array
        var iv = new byte[16];
        var encryptedKey = new byte[privateKey.KeySize / 8];
        var encryptedData = new byte[encryptedDataWithKey.Length - iv.Length - encryptedKey.Length];
        Buffer.BlockCopy(encryptedDataWithKey, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(encryptedDataWithKey, iv.Length, encryptedKey, 0, encryptedKey.Length);
        Buffer.BlockCopy(encryptedDataWithKey, iv.Length + encryptedKey.Length, encryptedData, 0, encryptedData.Length);

        // Decrypt the AES key with the RSA private key
        var key = privateKey.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

        // Decrypt the data with the AES key
        using var aes = Aes.Create();
        using var decryptor = aes.CreateDecryptor(key, iv);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);

        cs.Write(encryptedData, 0, encryptedData.Length);
        cs.FlushFinalBlock();

        var data = ms.ToArray();

        return data;
    }
}