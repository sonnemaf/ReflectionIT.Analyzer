using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullCheckAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0008";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string _category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            if (context == null)                 throw new ArgumentNullException(nameof(context));

            if (!(context is null)) {

            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNodeEquals, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNodeNotEquals, SyntaxKind.NotEqualsExpression);
        }

        private static void AnalyzeNodeEquals(SyntaxNodeAnalysisContext context) {
            var comp = (BinaryExpressionSyntax)context.Node;
            if (comp.Right.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Left.ToString(), "null", "==");
                context.ReportDiagnostic(diagnostic);
            } else if (comp.Left.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Right.ToString(), "null", "==");
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeNodeNotEquals(SyntaxNodeAnalysisContext context) {
            var comp = (BinaryExpressionSyntax)context.Node;
            if (comp.Right.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Left.ToString(), "not null", "!=");
                context.ReportDiagnostic(diagnostic);
            } else if (comp.Left.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Right.ToString(), "not null", "!=");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }


}
