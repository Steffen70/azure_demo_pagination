using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Basic.Reference.Assemblies;
using SPPaginationDemo.Extensions;

namespace SPPaginationDemo.ModelGenerator;

public class DynamicTypeGenerator
{
    public Type Model { get; private set; }
    public string AssemblyString { get; private set; }

    public static string GetIdentifier(string sqlQuery)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(sqlQuery));

        var hexString = string.Concat(hash.Select(b => b.ToString("X2")));

        return hexString;
    }

    public DynamicTypeGenerator(string sqlQuery, Type interfaceType, string connectionString, string contentRootPath)
    {
        var columns = AnalyzeQuery(sqlQuery, connectionString);
        var typeName = $"DynamicType_{GetIdentifier(sqlQuery)}";

        var template = File.ReadAllText(Path.Combine(contentRootPath, "dynamic_type_template.txt"));

        var properties = string.Join(Environment.NewLine, columns.Select(column =>
        {
            var (propertyName, propertyType) = column;
            var nullableType = propertyType.IsValueType ? $"Nullable<{propertyType.Name}>" : propertyType.Name;
            return $"public {nullableType} {propertyName} {{ get; set; }}";
        }));

        var code = template
            .Replace("[[Namespace]]", interfaceType.Namespace)
            .Replace("[[TypeName]]", typeName)
            .Replace("[[InterfaceName]]", interfaceType.Name)
            .Replace("[[Properties]]", properties);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references =
            new[] { Net70.References.SystemRuntime, Net70.References.SystemCore, MetadataReference.CreateFromFile(interfaceType.Assembly.Location) };

        var assemblyName = Path.GetRandomFileName();
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithPlatform(Platform.X86));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        // Todo: DS: Add error handling when compilation fails
        if (!result.Success)
            throw new Exception("Compilation failed");

        ms.Seek(0, SeekOrigin.Begin);
        var assemblyBytes = ms.ToArray();

        AssemblyString = Convert.ToBase64String(assemblyBytes.Compress());

        var assembly = Assembly.Load(assemblyBytes);

        Model = assembly.GetTypes().First(t => t.Name == typeName);
    }

    private static IEnumerable<(string ColumnName, Type DataType)> AnalyzeQuery(string sqlQuery, string connectionString)
    {
        using var connection = new SqlConnection(connectionString);

        connection.Open();

        using var command = new SqlCommand(sqlQuery, connection);

        using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

        var schemaTable = reader.GetSchemaTable();
        var columnNames = schemaTable!.Rows.OfType<DataRow>().Select(row => row["ColumnName"].ToString());
        var dataTypes = schemaTable.Rows.OfType<DataRow>().Select(row => row["DataType"] as Type);

        return columnNames.Zip(dataTypes, (columnName, dataType) => (ColumnName: columnName, DataType: dataType))
            .ToList()!;
    }
}