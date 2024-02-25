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

    StringBuilder GenerateClass(string className)
    {
        var source = new StringBuilder();

        source.AppendLine(@"
using System;

public partial class "
                    + className);

        source.AppendLine(@"{");
        AppendIndent(source);
        source.AppendLine($"public static {className} Instance {{ get; private set; }}");
        source.Append(@"}");
        return source;
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

                var generatedClass = GenerateClass(className);

                context.AddSource($"{className}_Singleton_g" , SourceText.From(generatedClass.ToString() , Encoding.UTF8));
            }
        }

    #endregion
    }
}