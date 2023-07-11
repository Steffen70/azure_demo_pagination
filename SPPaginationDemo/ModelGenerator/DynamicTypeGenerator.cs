using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace SPPaginationDemo.ModelGenerator;

public class DynamicTypeGenerator
{
    public Type Model { get; set; }

    public static string SqlQueryToIdentifier(string sqlQuery)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sqlQuery));
        var sb = new StringBuilder();
        foreach (var b in hash)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    public DynamicTypeGenerator(string sqlQuery, Type interfaceType, string connectionString)
    {
        var columns = AnalyzeQuery(sqlQuery, connectionString);
        var typeName = $"DynamicType_{Guid.NewGuid():N}";

        var assemblyName = new AssemblyName(typeName);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(typeName);

        var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
        typeBuilder.AddInterfaceImplementation(interfaceType);

        // Define the get and set methods for the property
        const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual;

        foreach (var (propertyName, propertyType) in columns)
        {
            var nullableType = propertyType.IsValueType ? typeof(Nullable<>).MakeGenericType(propertyType) : propertyType;

            // Define the fieldBuilder builder
            var fieldBuilder = typeBuilder.DefineField(propertyName, nullableType, FieldAttributes.Private);

            // Define the get method for the property
            var getter = typeBuilder.DefineMethod("get_" + propertyName, getSetAttr, nullableType, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();

            // Load 'this' onto the stack (the instance of the generated type)
            getterIl.Emit(OpCodes.Ldarg_0);

            // Emit the default value for the property type
            getterIl.Emit(OpCodes.Ldfld, fieldBuilder);

            // Return the value on the stack
            getterIl.Emit(OpCodes.Ret);

            // Define the set method for the property
            var setter = typeBuilder.DefineMethod("set_" + propertyName, getSetAttr, null, new[] { nullableType });
            var setterIl = setter.GetILGenerator();

            // Load 'this' onto the stack (the instance of the generated type)
            setterIl.Emit(OpCodes.Ldarg_0);

            // Load the property value onto the stack
            setterIl.Emit(OpCodes.Ldarg_1);

            // Store the value in the property field
            setterIl.Emit(OpCodes.Stfld, fieldBuilder);

            // Return
            setterIl.Emit(OpCodes.Ret);

            // Define the property builder
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, nullableType, null);

            // Assign the getter and setter methods to the property builder
            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);
        }
        Model = typeBuilder.CreateType();
    }

    private static List<(string ColumnName, Type DataType)> AnalyzeQuery(string sqlQuery, string connectionString)
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