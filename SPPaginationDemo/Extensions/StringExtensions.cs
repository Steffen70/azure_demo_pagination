using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

#pragma warning disable SYSLIB1045

namespace SPPaginationDemo.Extensions;

public static class StringExtensions
{
    public static string CamelCaseToSnakeCase(this string str) => Replace(str, @"([A-Z])", "_$1", RegexOptions.Compiled).Trim('_').ToLower();

    public static string NormalizeTypeName(this string str)
    {
        str = str.Replace(".", string.Empty);

        str = str.Split('>')[0];

        str = str.Replace("get_", string.Empty);

        str = str.Replace("<", string.Empty);

        return str;
    }

    public static string GenerateMd5Hash(this string str)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));

        var hexString = string.Concat(hash.Select(b => b.ToString("X2")));

        return hexString;
    }
}