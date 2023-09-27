using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SPPaginationDemo.CallLogger;
using SPPaginationDemo.Extensions;
using SPPaginationDemo.Services;
#pragma warning disable CA2254

#pragma warning disable IDE0290

namespace SPPaginationDemo.DtoGenerator;

public class SqlGeneratorFactory : BaseFactory
{
    private readonly string _actionName;
    private readonly Type _interfaceType;
    private readonly Appsettings _appsettings;

    // Todo: DS: Check update time of the sql query file and update the cache if it has changed.
    [Log]
    public string SqlQuery => MemoryCache.LazyLoadAndCache($"{_actionName}_Query", () => File.ReadAllText(Path.Combine(_appsettings.ContentRootPath, "SqlQueries", $"{_actionName}.sql")));

    [Log]
    public override string AssemblyString => MemoryCache.LazyLoadAndCache(SqlIdentifier, GetAssemblyString);

    [Log]
    private string GetAssemblyString()
    {
        var columns = AnalyzeQuery(SqlQuery, _appsettings.SqlConnectionString);

        var template = File.ReadAllText(Path.Combine(_appsettings.ContentRootPath, "CodeTemplates", "dynamic_type.txt"));

        var properties = string.Join(Environment.NewLine, columns.Select(column =>
        {
            var (propertyName, propertyType) = column;
            var nullableType = propertyType.IsValueType ? $"Nullable<{propertyType.Name}>" : propertyType.Name;
            return $"public {nullableType} {propertyName} {{ get; set; }}";
        }));

        var code = template
            .Replace("[[Namespace]]", _interfaceType.Namespace)
            .Replace("[[TypeName]]", TypeName)
            .Replace("[[InterfaceName]]", _interfaceType.Name)
            .Replace("[[Properties]]", properties);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references =
            new[]
            {
                Net70.References.SystemRuntime, Net70.References.SystemCore,
                MetadataReference.CreateFromFile(_interfaceType.Assembly.Location)
            };

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

        var assemblyString = Convert.ToBase64String(assemblyBytes.Compress());

        Logger.LogInformation($"Generated new assembly for '{_actionName}' with type '{TypeName}'");

        return assemblyString;
    }

    // Todo: DS: Check update timestamp of sql file and recompile if changed
    [Log]
    public override string SqlIdentifier => MemoryCache.LazyLoadAndCache(_actionName, GetSqlIdentifier);

    [Log]
    private string GetSqlIdentifier()
    {
        var hexString = SqlQuery.GenerateMd5Hash();

        Logger.LogInformation($"Generated new SqlIdentifier '{hexString}' for '{_actionName}'");

        return hexString;
    }

    [Log]
    public SqlGeneratorFactory(ILogger logger, Appsettings appsettings, string actionName, Type interfaceType) : base(logger)
    {
        _appsettings = appsettings;

        _actionName = actionName.CamelCaseToSnakeCase();
        _interfaceType = interfaceType;
    }

    [Log]
    private IEnumerable<(string ColumnName, Type DataType)> AnalyzeQuery(string sqlQuery, string connectionString)
    {
        using var connection = new SqlConnection(connectionString);

        connection.Open();

        using var command = new SqlCommand(sqlQuery, connection);

        using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

        var schemaTable = reader.GetSchemaTable();
        var columnNames = schemaTable!.Rows.OfType<DataRow>().Select(row => row["ColumnName"].ToString());
        var dataTypes = schemaTable.Rows.OfType<DataRow>().Select(row => row["DataType"] as Type);

        Logger.LogInformation($"Analyzed query '{SqlIdentifier}'");

        return columnNames.Zip(dataTypes, (columnName, dataType) => (ColumnName: columnName, DataType: dataType))
            .ToList()!;
    }
}