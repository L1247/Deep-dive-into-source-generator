using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnitySourceGenerator
{
    [Generator]
    public class GetComponentGenerator : ISourceGenerator
    {
    #region Private Variables

        private const string _attributeText = @"
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal class GetComponentAttribute : Attribute
{
    public enum TargetType
    {
        This = 0, // Only Self 從自己身上拿
        Parent = 1, // Inculde self 包含自己
        Child = 2, // Inculde Self 包含自己
        ParentExcludeSelf = 3, // 不包含自己
        ChildExcludeSelf = 4, // 不包含自己
    }

    public enum InActiveType
    {
        /// <summary>
        /// 不包含隱藏
        /// </summary>
        Exclude ,

        /// <summary>
        /// 包含隱藏
        /// </summary>
        Include
    }

    public GetComponentAttribute(TargetType targetType = TargetType.This , InActiveType inactiveType = InActiveType.Exclude)
    {
    }
}
";

    #endregion

    #region Public Methods

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) return;

            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("GetComponentAttribute");

            foreach (IGrouping<INamedTypeSymbol , IFieldSymbol> group in receiver.Fields
                                                                                 .GroupBy<IFieldSymbol , INamedTypeSymbol>(
                                                                                          f => f.ContainingType ,
                                                                                          SymbolEqualityComparer.Default))
            {
                var classSource = ProcessClass(group.Key , group , attributeSymbol);
                context.AddSource($"{group.Key.Name}_Components_g.cs" , SourceText.From(classSource , Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization
                    (i => i.AddSource("GetComponentAttribute_g.cs" , _attributeText));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

    #endregion

    #region Private Methods

        private static void AppendIndent(StringBuilder source , int indentLevel = 1)
        {
            source.Append($"{new string(' ' , indentLevel * 4)}");
        }

        private string ProcessClass(ISymbol classSymbol , IEnumerable<IFieldSymbol> fields , ISymbol attributeSymbol)
        {
            var source = new StringBuilder($@"
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine;
using rStarUtility;

public partial class {classSymbol.Name} 
{{
    private void InitializeComponents()
    {{
");

            foreach (var fieldSymbol in fields)
            {
                ProcessField(classSymbol.Name , source , fieldSymbol , attributeSymbol);
            }

            AppendIndent(source);
            source.AppendLine("}");
            source.AppendLine("}");
            return source.ToString();
        }

        private void ProcessField(string classSymbolName , StringBuilder source , IFieldSymbol fieldSymbol , ISymbol attributeSymbol)
        {
            var fieldName = fieldSymbol.Name;
            var fieldType = fieldSymbol.Type;

            var attributeData = fieldSymbol.GetAttributes()
                                           .Single(ad => ad.AttributeClass.Equals(attributeSymbol ,
                                                                                  SymbolEqualityComparer.Default));

            var getComponent = ProcessGetComponent(attributeData , fieldName , fieldType);
            var assertion    = ProcessAssertion(classSymbolName , fieldName);
            AppendIndent(source , 2);
            source.AppendLine(getComponent);
            AppendIndent(source , 2);
            source.AppendLine(assertion);
        }

        private string ProcessAssertion(string classSymbolName , string fieldName)
        {
            var          source            = new StringBuilder();
            const string gameObjectMessage = "<color=#FFAF0F>GameObject - [{gameObject.name}]</color>";
            var          fieldMessage      = $"<color=red>Field - [{fieldName}]</color>";
            var          classMessage      = $"<color=#5995ed>Component - [{classSymbolName}]</color>";
            var          message           = "$" + $"\"{fieldMessage} is null in {classMessage} from {gameObjectMessage}\"";
            source.Append($"Assert.IsNotNull({fieldName} , {message});");
            return source.ToString();
        }

        private string ProcessGetComponent(AttributeData attributeData , string fieldName , ITypeSymbol fieldType)
        {
            var methodBuilder = new StringBuilder("GetComponent");
            var excludeMethod = string.Empty;
            if (attributeData.ConstructorArguments.Length > 0
             && int.TryParse(attributeData.ConstructorArguments[0].Value.ToString() , out var componentValue)
             && int.TryParse(attributeData.ConstructorArguments[1].Value.ToString() , out var inactiveValue))
            {
                var inactive = inactiveValue switch
                {
                    // exclude
                    0 => "Inactive.Exclude" ,
                    // include
                    1 => "Inactive.Include" ,
                    // default Exclude inactive
                    _ => "Inactive.Exclude"
                };

                switch (componentValue)
                {
                    case 1 : // Parent
                        methodBuilder.Append("InParent");
                        break;
                    case 2 : // Child
                        methodBuilder.Append("InChildren");
                        break;
                    case 3 or 4 : // Exclude parent or child
                        return $"{fieldName} = this.GetComponentInChildren<{fieldType}>(Self.Exclude , {inactive});";
                }
            }

            var methodType       = methodBuilder.ToString();
            var componentBuilder = new StringBuilder();
            componentBuilder.Append($@"{fieldName} = {methodType}<{fieldType}>(){excludeMethod};");
            return componentBuilder.ToString();
        }

    #endregion
    }

    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
    #region Public Variables

        public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

    #endregion

    #region Public Methods

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax && fieldDeclarationSyntax.AttributeLists.Count > 0)
            {
                foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                {
                    IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;

                    if (IsDerivedFrom(fieldSymbol?.ContainingType.BaseType , "MonoBehaviour")
                     && IsDerivedFrom(fieldSymbol?.Type.BaseType ,           "Component")
                     && fieldSymbol.GetAttributes()
                                   .Any(ad => ad.AttributeClass.ToDisplayString() == "GetComponentAttribute"))
                    {
                        Fields.Add(fieldSymbol);
                    }
                }
            }
        }

    #endregion

    #region Private Methods

        private bool IsDerivedFrom(INamedTypeSymbol baseType , string targetType)
        {
            while (baseType != null)
            {
                if (baseType.Name == targetType) return true;

                baseType = baseType.BaseType;
            }

            return false;
        }

    #endregion
    }
}