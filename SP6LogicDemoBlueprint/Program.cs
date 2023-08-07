using System.Diagnostics;
using System.Reflection;
using System.Text;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

// Explore types and methods in the assembly using reflection
foreach (var type in assembly.GetTypes().Where(t => t.Namespace == assemblyNamespace))
{
    var className = type.Name;

    var newClassDeclaration = SyntaxFactory.ClassDeclaration(className)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    // TODO: maybe remove DeclaredOnly to get inherited methods
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var returnType = method.ReturnType.Name;
        if (method.ReturnType.Namespace != null)
            returnType = $"{method.ReturnType.Namespace}.{returnType}";

        var newMethodDeclaration = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), method.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(SyntaxFactory.ThrowStatement(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName("System.NotImplementedException"),
                    SyntaxFactory.ArgumentList(),
                    null))));

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

// 2. Create the namespace declaration.
var newNamespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(assemblyNamespace))
    .AddMembers(allMembers.ToArray());

// 3. Combine the attribute and the namespace into a single compilation unit.
var compilationUnit = SyntaxFactory.CompilationUnit()
    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
    .AddAttributeLists(assemblyAttributes.ToArray())
    .AddMembers(newNamespaceDeclaration);

var formattedCode = compilationUnit.NormalizeWhitespace().ToFullString();
var sourceText = SourceText.From(formattedCode, Encoding.UTF8);
var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);

var refenrences = new[] { Net70.References.SystemRuntime, Net70.References.SystemCore };

var assemblyName = assemblyNamespace.Split('.').Last();
var compilation = CSharpCompilation.Create(
    assemblyName,
    syntaxTrees: new[] { syntaxTree },
    references: refenrences,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithPlatform(Platform.X64));

using var ms = new MemoryStream();
var result = compilation.Emit(ms);

//TODO: Add error handling when compilation fails
if (!result.Success)
    throw new Exception("Compilation failed");

var outputPath = Path.Combine(currentAssemblyPath, $"Blueprint_{assemblyName}.dll");

// write memory stream to file
using var fs = new FileStream(outputPath, FileMode.Create);
ms.Seek(0, SeekOrigin.Begin);
ms.CopyTo(fs);
fs.Close();