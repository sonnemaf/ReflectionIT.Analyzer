using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PrivateFieldCodeFixProvider)), Shared]
    public class NullCheckCodeFixProvider : CodeFixProvider {
        private const string title = "Fix null check";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(NullCheckAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceEqualsWithIsAsync(context.Document, root, declaration),
                    equivalenceKey: title),
                diagnostic);


        }

        private Task<Document> ReplaceEqualsWithIsAsync(Document document, SyntaxNode root, BinaryExpressionSyntax comp) {

            SyntaxNode newOperatorToken =  SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, comp.Left, comp.Right);

            if (comp.OperatorToken.Kind() == SyntaxKind.ExclamationEqualsToken) {
                newOperatorToken = SyntaxFactory.ParenthesizedExpression(newOperatorToken as ExpressionSyntax);
                newOperatorToken = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, newOperatorToken as ExpressionSyntax);
            }

            /// Replace old with new
            var newRoot = root.ReplaceNode(comp, newOperatorToken);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}