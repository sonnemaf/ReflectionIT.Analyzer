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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCheckCodeFixProvider)), Shared]
    public class NullCheckCodeFixProvider : CodeFixProvider {
        private const string Title = "Fix null check";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NullCheckAnalyzer.DiagnosticId);

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
                    title: Title,
                    createChangedDocument: c => ReplaceEqualsWithIsAsync(context.Document, declaration, context.CancellationToken),
                    equivalenceKey: Title),
                diagnostic);


        }

        private async Task<Document> ReplaceEqualsWithIsAsync(Document document, BinaryExpressionSyntax comp, CancellationToken c) {

            var left = (comp.Right.Kind() == SyntaxKind.NullLiteralExpression) ? comp.Left : comp.Right;
            var right = (comp.OperatorToken.Kind() == SyntaxKind.ExclamationEqualsToken) ? SyntaxFactory.ParseExpression("object") : SyntaxFactory.ParseExpression("null");

            SyntaxNode newOperatorToken = SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, left, right);

            /// Replace old with new
            var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(comp, newOperatorToken);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}