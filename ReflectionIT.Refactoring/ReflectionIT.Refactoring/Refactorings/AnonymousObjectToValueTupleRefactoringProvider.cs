using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReflectionIT.Refactoring.Refactorings {

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AnonymousObjectToValueTupleRefactoringProvider)), Shared]
    internal class AnonymousObjectToValueTupleRefactoringProvider : CodeRefactoringProvider {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a type declaration node.
            if (!(node is AnonymousObjectCreationExpressionSyntax anonymousObjectCreation)) {
                return;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Convert Anonymous Object to ValueTuple", c => ReverseTypeNameAsync(context.Document, anonymousObjectCreation, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Document> ReverseTypeNameAsync(Document document, AnonymousObjectCreationExpressionSyntax anonymousObjectCreation, CancellationToken cancellationToken) {
            // Crate Tuple
            var te = SyntaxFactory.TupleExpression();

            // Add arguments to Tuple
            var arguments = anonymousObjectCreation.Initializers.Select(it => it.NameEquals == null
                    ? SyntaxFactory.Argument(it.Expression)
                    : SyntaxFactory.Argument(SyntaxFactory.NameColon(it.NameEquals.Name.ToString()), SyntaxFactory.Token(SyntaxKind.None), it.Expression));
            te = te.AddArguments(arguments.ToArray());

            // Replace old (AnonymousObject) with the new (Tuple)
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken)
                                        .ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(anonymousObjectCreation, te);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
