using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DependencyProperty.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DependencyPropertyAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DependencyPropertyAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static bool IsLastPartOfNameSpace(string nameSpace, string name)
        {
            return nameSpace.Split('.').Last() == name;
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            if (memberAccess.Name.ToString() == "Register")
            {

                if (!IsLastPartOfNameSpace(memberAccess.Expression.ToString(), "DependencyProperty"))
                {
                    return;
                }
                var parent = memberAccess.Parent;

                var argumentList = parent.ChildNodes().FirstOrDefault(child => child is ArgumentListSyntax);
                if (argumentList == null)
                {
                    return;
                }

                var argument = argumentList.ChildNodes().FirstOrDefault(child => child is ArgumentSyntax);
                if (argument == null)
                {
                    return;
                }

                var stringLiteral = argument.ChildNodes().FirstOrDefault(child => child is LiteralExpressionSyntax && child.Kind() == SyntaxKind.StringLiteralExpression);

                if (stringLiteral == null)
                {
                    return;
                }

                var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), stringLiteral.ToString().Trim('\"'));
                context.ReportDiagnostic(diagnostic);
            }

        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
