using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class EmptyArrayAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0010";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.EmptyArrayAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.EmptyArrayAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.EmptyArrayAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string _category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(startContext => {
                var arrayType = startContext.Compilation.GetTypeByMetadataName("System.Array");
                if (arrayType?.GetMembers("Empty").Length > 0) {
                    startContext.RegisterOperationAction(AnalyzeArrayCreation, OperationKind.ArrayCreation);
                }
            });
        }

        private void AnalyzeArrayCreation(OperationAnalysisContext context) {
            var arrayCreation = (IArrayCreationOperation)context.Operation;
            if (arrayCreation.DimensionSizes.Length == 1 && arrayCreation.DimensionSizes[0].ConstantValue.HasValue) {

                var dim = arrayCreation.DimensionSizes[0].ConstantValue.Value;
                if (dim is 0) {
                    var diagnostic = Diagnostic.Create(_rule, arrayCreation.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        
    }


}
