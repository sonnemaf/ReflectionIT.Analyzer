using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace ReflectionIT.Analyzer.Analyzers.AsyncMethodNameSuffix {

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodNameSuffixAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0002";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AsyncMethodNameSuffixAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AsyncMethodNameSuffixAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AsyncMethodNameSuffixAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = DiagnosticAnalyzerCategories.Naming;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context) {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var methodSymbol = (IMethodSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (methodSymbol.IsAsync &&
                !methodSymbol.ReturnsVoid &&
                !methodSymbol.IsOverride &&
                !methodSymbol.Name.EndsWith("Async")) {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rule, methodSymbol.Locations[0], methodSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }


    }
}
