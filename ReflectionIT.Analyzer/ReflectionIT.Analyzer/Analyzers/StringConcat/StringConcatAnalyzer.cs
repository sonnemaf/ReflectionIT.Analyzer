using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ReflectionIT.Analyzer.Analyzers.StringConcat {

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringConcatAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0011";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static LocalizableString Title { get; } = new LocalizableResourceString(nameof(Resources.StringConcatAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static LocalizableString MessageFormat { get; } = new LocalizableResourceString(nameof(Resources.StringConcatAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static LocalizableString Description { get; } = new LocalizableResourceString(nameof(Resources.StringConcatAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string _category = DiagnosticAnalyzerCategories.Performance;
        private static DiagnosticDescriptor Rule { get; } = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeAddExpression, SyntaxKind.AddExpression);
        }

        private void AnalyzeAddExpression(SyntaxNodeAnalysisContext context) {
            if (context.Node is BinaryExpressionSyntax addExpression) {
                var retType = context.SemanticModel.GetTypeInfo(addExpression);
                if (retType.Type.SpecialType == SpecialType.System_String) context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

    }
}
