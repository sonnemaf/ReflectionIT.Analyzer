using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ReflectionIT.Analyzer.Analyzers.AutoPropertiesInStructs {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutoPropertiesInStructsAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0009";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AutoPropertiesInStructsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AutoPropertiesInStructsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AutoPropertiesInStructsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string _category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var property = (PropertyDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;

            // Find locals (not constants) with invalid names
            if (symbol.ContainingType.IsValueType &&
                property.AccessorList?.Accessors.All(a => a.Body == null) == true) {

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rule, property.Identifier.GetLocation(), property.Identifier.Text);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
