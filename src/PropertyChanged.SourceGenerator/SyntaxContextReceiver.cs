using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PropertyChanged.SourceGenerator.Analysis;

namespace PropertyChanged.SourceGenerator;

public class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public HashSet<INamedTypeSymbol> Types { get; } = new(SymbolEqualityComparer.Default);

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        switch (context.Node)
        {
            case FieldDeclarationSyntax fieldDeclaration:
                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    Process(variable);
                }
                break;
            case PropertyDeclarationSyntax propertyDeclaration:
                Process(propertyDeclaration);
                break;
        }

        void Process(SyntaxNode node)
        {
            if (context.SemanticModel.GetDeclaredSymbol(node) is not { } symbol) return;

            bool flag = false;
            
            foreach (var attributeData in symbol.GetAttributes())
            {
                if(attributeData.AttributeClass?.ContainingNamespace.ToDisplayString() != "PropertyChanged.SourceGenerator") continue;
                flag = true;

                if(attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attrSyntax) continue;
                
                if(attrSyntax.Name.GetFirstToken().ValueText != "DependsOn") continue;
                if(attrSyntax.ArgumentList is not {} argumentList) continue;

                string?[] arguments = argumentList.Arguments.Select(x => ParseNameOf(x.GetText().ToString().AsSpan())).ToArray();

                Analyser.DependsOnAttributeNameOfArguments[attributeData] = arguments;
            }

            if (flag) this.Types.Add(symbol.ContainingType);
        }


        string? ParseNameOf(ReadOnlySpan<char> text)
        {
            if (!text.StartsWith("nameof(".AsSpan()) || !text.EndsWith(")".AsSpan())) return null;

            var memberPath = text.Slice(7, text.Length - 8);
            int lastPointIndex = memberPath.LastIndexOf('.');
            if (lastPointIndex < 0) return memberPath.ToString();

            return memberPath.Slice(lastPointIndex + 1).ToString();
        }
        
    }
}
