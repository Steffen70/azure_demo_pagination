using System.Diagnostics;
using System.Reflection;
using System.Text;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

#if DEBUG
Debugger.Launch();
#endif

var assemblyNamespace = args[0];

var currentAssembly = Assembly.GetCallingAssembly();
var currentAssemblyPath = Path.GetDirectoryName(currentAssembly.Location)!;

var targetAssemblyPath = Path.Combine(currentAssemblyPath, $"{assemblyNamespace}.dll");

// assembly from references by namespace
var assembly = Assembly.LoadFrom(targetAssemblyPath);

var allMembers = new List<MemberDeclarationSyntax>();

// load methodbody template from method_body_template.txt
var methodBodyTemplate = File.ReadAllText(Path.Combine(currentAssemblyPath, "method_body_template.txt"));

// Explore types and methods in the assembly using reflection
foreach (var type in assembly.GetTypes().Where(t => t.Namespace == assemblyNamespace))
{
    var typeName = type.Name;

    var newClassDeclaration = SyntaxFactory.ClassDeclaration(typeName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    // Todo: DS: maybe remove DeclaredOnly to get inherited methods
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var returnType = $"{method.ReturnType.Namespace}.{method.ReturnType.Name}";

        // Collecting parameters into a dictionary named paramValues
        var paramValuesCreation = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("var"))
                .AddVariables(SyntaxFactory.VariableDeclarator("paramValues")
                    .WithInitializer(SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("Dictionary<string, object>"))
                            .WithArgumentList(SyntaxFactory.ArgumentList())))));

        var addStatements = method.GetParameters()
            .Select(param =>
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.ParseExpression($"paramValues.Add(\"{param.Name}\", {param.Name})")))
            .ToArray();

        var statementLines = methodBodyTemplate
            .Replace("[typeName]", $"{type.Namespace}.{typeName}")
            .Replace("[methodName]", method.Name)
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        var statementsList = statementLines
            .Select(line => SyntaxFactory.ParseStatement(line).WithTrailingTrivia(SyntaxFactory.CarriageReturn))
            .ToList();

        // Todo: DS: add support for async methods

        // else if return type is string add return statement
        if (method.ReturnType == typeof(string))
            statementsList.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression("stringResult!")));

        // add return statement if return type is serializable and not primitive type
        else if (method.ReturnType is { IsSerializable: true, IsPrimitive: false })
            statementsList.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"JsonSerializer.Deserialize<{returnType}>(stringResult)!")));
        
        // Todo: DS: add support for nullable types

        // else if return type is not void add return statement try to parse the result
        else if (method.ReturnType != typeof(void))
            statementsList.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"{returnType}.Parse(stringResult!)")));

        var methodBody = SyntaxFactory.Block(new List<StatementSyntax>
        {
            paramValuesCreation
        }.Concat(addStatements).Concat(statementsList));

        var newMethodDeclaration = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), method.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(methodBody);

        newClassDeclaration = newClassDeclaration.AddMembers(newMethodDeclaration);
    }

    allMembers.Add(newClassDeclaration);
}

var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

// 1. Create the assembly version attribute.
var versionStringLiteral = SyntaxFactory.Literal(currentAssemblyVersion.ToString());

var assemblyVersionAttribute = SyntaxFactory.Attribute(
    SyntaxFactory.ParseName("System.Reflection.AssemblyVersion"),
    SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, versionStringLiteral))))
);

var assemblyAttributes = SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(assemblyVersionAttribute)).WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))));

var usingDirectives = new List<UsingDirectiveSyntax>
{
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http")),
    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http.Headers")),
};

var helperClassCode = File.ReadAllText(Path.Combine(currentAssemblyPath, "web_client_template.txt"));
allMembers.Add(SyntaxFactory.ParseMemberDeclaration(helperClassCode)!);

// 2. Create the namespace declaration.
var newNamespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(assemblyNamespace))
    .AddMembers(allMembers.ToArray());

// 3. Combine the attribute and the namespace into a single compilation unit.
var compilationUnit = SyntaxFactory.CompilationUnit()
    .AddUsings(usingDirectives.ToArray())
    .AddAttributeLists(assemblyAttributes.ToArray())
    .AddMembers(newNamespaceDeclaration);

var formattedCode = compilationUnit.NormalizeWhitespace().ToFullString();
var sourceText = SourceText.From(formattedCode);
var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);

var refenrences = new[] {
    Net70.References.SystemRuntime,
    Net70.References.SystemCore,
    Net70.References.SystemCollections,
    Net70.References.SystemLinq,
    Net70.References.SystemNetHttp,
    Net70.References.SystemNetPrimitives,
    Net70.References.SystemTextJson
};

var assemblyName = assemblyNamespace.Split('.').Last();
var compilation = CSharpCompilation.Create(
    assemblyName,
    syntaxTrees: new[] { syntaxTree },
    references: refenrences,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithPlatform(Platform.X86)
        .WithNullableContextOptions(NullableContextOptions.Enable)
#if RELEASE
        .WithOptimizationLevel(OptimizationLevel.Release));
#endif
#if DEBUG
        .WithOptimizationLevel(OptimizationLevel.Debug));
#endif

using var assemblyStream = new MemoryStream();

#if DEBUG
using var pdbStream = new MemoryStream();
#endif

var result = compilation.Emit(
    peStream: assemblyStream
#if RELEASE
    );
#endif
#if DEBUG
    , pdbStream: pdbStream);
#endif

// Todo: DS: Add error handling when compilation fails
if (!result.Success)
    throw new Exception("Compilation failed");

var blueprintName = $"Blueprint_{assemblyName}";
var assemblyPath = Path.Combine(currentAssemblyPath, $"{blueprintName}.dll");

// write assemblyStream to file
using var assemblyFileStream = new FileStream(assemblyPath, FileMode.Create);
assemblyStream.Seek(0, SeekOrigin.Begin);
assemblyStream.CopyTo(assemblyFileStream);
assemblyFileStream.Close();


#if DEBUG
// write pdbStream to file
var pdbPath = Path.Combine(currentAssemblyPath, $"{blueprintName}.pdb");
using var pdbFileStream = new FileStream(pdbPath, FileMode.Create);
pdbStream.Seek(0, SeekOrigin.Begin);
pdbStream.CopyTo(pdbFileStream);
pdbFileStream.Close();
#endif
