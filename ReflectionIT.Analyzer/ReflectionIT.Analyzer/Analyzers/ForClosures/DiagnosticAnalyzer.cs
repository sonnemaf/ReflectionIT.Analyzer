using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace ReflectionIT.Analyzer.Analyzers.ForClosure {

    // Source: https://channel9.msdn.com/events/TechDays/Techdays-2016-The-Netherlands/Introduction-to-building-code-analyzers-and-code-fixes-with-Roslyn

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForClosureAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0005";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.ForClosureAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.ForClosureAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.ForClosureAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = DiagnosticAnalyzerCategories.Naming;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ForStatement);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var fss = (ForStatementSyntax)context.Node;

            var sem = context.SemanticModel;
            if (sem != null) {
                var variables = fss.Declaration.Variables;

                var symbols = new HashSet<ISymbol>(variables.Select(variable => sem.GetDeclaredSymbol(variable)));

                var johnny = new Johnny(context, sem, symbols);

                johnny.Visit(fss.Statement);
            }
        }

        class Johnny : CSharpSyntaxWalker {
            private readonly SyntaxNodeAnalysisContext _context;
            private readonly SemanticModel _model;
            private readonly HashSet<ISymbol> _symbols;

            public Johnny(SyntaxNodeAnalysisContext context, SemanticModel model, HashSet<ISymbol> symbols) {
                _context = context;
                _model = model;
                _symbols = symbols;
            }

            private bool _inLambda;

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
                var wasInLambda = _inLambda;
                _inLambda = true;

                base.VisitParenthesizedLambdaExpression(node);

                _inLambda = wasInLambda;
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
                var wasInLambda = _inLambda;
                _inLambda = true;

                base.VisitSimpleLambdaExpression(node);

                _inLambda = wasInLambda;
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) {
                var wasInLambda = _inLambda;
                _inLambda = true;

                base.VisitAnonymousMethodExpression(node);

                _inLambda = wasInLambda;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node) {
                if (_inLambda) {
                    var symbol = _model.GetSymbolInfo(node);
                    if (symbol.Symbol != null && _symbols.Contains(symbol.Symbol)) {
                        var diagnostic = Diagnostic.Create(_rule, node.GetLocation(), node.Identifier.Text);

                        _context.ReportDiagnostic(diagnostic);
                    }
                }

                base.VisitIdentifierName(node);
            }
        }

    }
}
