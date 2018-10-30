using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Analyzers.OptionalParameterInPublicClassPublicMethod {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionalParameterInPublicClassPublicMethodAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0007";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.OptionalParameterInPublicClassPublicMethodAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.OptionalParameterInPublicClassPublicMethodAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.OptionalParameterInPublicClassPulicMethodAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Parameter);
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Parameter);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var parameter = (ParameterSyntax)context.Node;
            var method = parameter.Parent.Parent as BaseMethodDeclarationSyntax;
            var accessibility = context.SemanticModel.GetDeclaredSymbol(method).DeclaredAccessibility;
            var type = context.SemanticModel.GetDeclaredSymbol(method).ContainingSymbol;


            // Find locals (not constants) with invalid names
            if (parameter.Default != null && method != null && !Allowed(accessibility) && !Allowed(type.DeclaredAccessibility)) {

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rule, parameter.GetLocation(), parameter);

                context.ReportDiagnostic(diagnostic);

            }

            bool Allowed(Accessibility a) {
                return a == Accessibility.Private || a == Accessibility.Friend || a == Accessibility.Internal || a == Accessibility.ProtectedAndFriend || a == Accessibility.ProtectedAndInternal;
            }
        }


        public void Test(int a = 5) {

        }
    }
}
