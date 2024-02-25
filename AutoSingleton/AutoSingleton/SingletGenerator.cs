using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoSingleton;

[Generator]
public class SingletGenerator : ISourceGenerator
{
#region Private Variables

    private const string attributeText = @"
using System;

[AttributeUsage(AttributeTargets.Class , Inherited = true , AllowMultiple = false)]
internal class SingletonAttribute : Attribute
{
}
";

#endregion

#region Public Methods

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization
                (i => i.AddSource("SingletonAttribute_g.cs" , attributeText));
        Console.WriteLine("SingletonAttribute");
    }

#endregion

#region Public Methods

    string GenerateClass(string className , int indentLevel)
    {
        var source = new StringBuilder();
        AppendIndent(source , indentLevel);
        source.Append(@"public partial class " + className);
        source.AppendLine(@"{");
        AppendIndent(source , 1 + indentLevel);
        source.AppendLine($"private static {className} instance;");
        AppendIndent(source , 1 + indentLevel);
        source.AppendLine($"public static {className} Instance => instance ??= new {className}();");
        AppendIndent(source , indentLevel);
        source.Append(@"}");
        return source.ToString();
    }

    private static void AppendIndent(StringBuilder source , int indentLevel = 1)
    {
        source.Append($"{new string(' ' , indentLevel * 4)}");
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxTrees = context.Compilation.SyntaxTrees;
        foreach (var syntaxTree in syntaxTrees)
        {
            var singleTypeDeclarations =
                    syntaxTree.GetRoot()
                              .DescendantNodes()
                              .OfType<TypeDeclarationSyntax>()
                              .Where(x => x.AttributeLists.Any(xx => xx.ToString().StartsWith("[Singleton")))
                              .ToList();
            foreach (var singleTypeDeclaration in singleTypeDeclarations)
            {
                var className = singleTypeDeclaration.Identifier.ToString();
                var nameSpace = GetNamespace(singleTypeDeclaration);

                int indentLevel               = nameSpace == string.Empty ? 0 : 1;
                var generatedClass            = GenerateClass(className , indentLevel);
                var generateNameSpaceAndClass = GenerateNameSpace(generatedClass , nameSpace);

                var sourceText = SourceText.From(generateNameSpaceAndClass , Encoding.UTF8);
                context.AddSource($"{className}_Singleton_g" , sourceText);
            }
        }

    #endregion
    }

    // determine the namespace the class/enum/struct is declared in, if any
    // https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/#finding-the-namespace-for-a-class-syntax
    static string GetNamespace(SyntaxNode syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        var nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        var potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null
            && potentialNamespaceParent is not NamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace       = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    private string GenerateNameSpace(string classContent , string nameSpace)
    {
        if (nameSpace == string.Empty) return classContent;
        var source = new StringBuilder();
        source.AppendLine($@"
namespace {nameSpace}
{{
{classContent}
}}");

        return source.ToString();
    }
}