using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Analyzers.NonPrivateField {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPrivateFieldAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0003";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.NonPrivateFieldAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.NonPrivateFieldAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.NonPrivateFieldAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string _category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var field = (IFieldSymbol)context.Symbol;

            //var parentType = field.ContainingType.IsReferenceType;

            // Find fields (not constants) with invalid names
            if (field.DeclaredAccessibility != Accessibility.Private && !field.IsConst && !field.IsReadOnly && field.ContainingType.IsReferenceType) {

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rule, field.Locations[0], field.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
