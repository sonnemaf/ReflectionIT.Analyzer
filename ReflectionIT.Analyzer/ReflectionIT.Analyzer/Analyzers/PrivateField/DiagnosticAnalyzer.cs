using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrivateFieldAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.Naming;

        private static DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            // Find fields (not constants) with invalid names
            if (fieldSymbol.DeclaredAccessibility == Accessibility.Private &&
                !fieldSymbol.IsConst && !string.IsNullOrWhiteSpace(fieldSymbol.Name) &&
                fieldSymbol.Name[0] != '_' ) {

                string name = GetCorrectFieldName(fieldSymbol.Name);

                if (fieldSymbol.Name != name) {

                    // For all such symbols, produce a diagnostic.
                    var diagnostic = Diagnostic.Create(_rule, fieldSymbol.Locations[0], fieldSymbol.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        internal static string GetCorrectFieldName(string name) {

            if (name.StartsWith("m_") && name.Length > 2) {
                name = "_" + name[2].ToString().ToLower() + name.Substring(3);
            } else {
                name = "_" + name[0].ToString().ToLower() + name.Substring(1);
            }

            return name;
        }
    }
}
