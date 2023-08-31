using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

#pragma warning disable SYSLIB1045

namespace SPPaginationDemo.Extensions;

public static class StringExtensions
{
    public static string CamelCaseToSnakeCase(this string str) => Replace(str, @"([A-Z])", "_$1", RegexOptions.Compiled).Trim('_').ToLower();

    public static Stream GetStream(this string str) => new MemoryStream(Encoding.UTF8.GetBytes(str));
}