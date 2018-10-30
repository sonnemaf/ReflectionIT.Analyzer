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
    public class NullCheckAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0008";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.NullCheckAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            if (!(context is null)) {
                
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNodeEquals, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNodeNotEquals, SyntaxKind.NotEqualsExpression);
        }

        private static void AnalyzeNodeEquals(SyntaxNodeAnalysisContext context) {
            var comp = (BinaryExpressionSyntax)context.Node;
            if (comp.Right.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Left.ToString(), "==", string.Empty, string.Empty);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeNodeNotEquals(SyntaxNodeAnalysisContext context) {
            var comp = (BinaryExpressionSyntax)context.Node;
            if (comp.Right.Kind() == SyntaxKind.NullLiteralExpression) {
                var diagnostic = Diagnostic.Create(_rule, comp.GetLocation(), comp.Left.ToString(), "!=", "!(", ")");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

   
}
