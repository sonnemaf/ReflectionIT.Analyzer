using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using ReflectionIT.Analyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReflectionIT.Analyzer.Refactorings {
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged)), Shared]
    public class AutoPropertyToFullProperty : CodeRefactoringProvider {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a PropertyDeclarationSyntax
            if (!(node is PropertyDeclarationSyntax propDecl)) {
                return;
            }

            // Only offer a refactoring if the selected node is a AUTO property
            if (propDecl.AccessorList == null || propDecl.AccessorList.Accessors.All(a => a.ChildNodes().OfType<BlockSyntax>().Any()) || propDecl.AccessorList.Accessors.Count != 2) {
                return;
            }

            // Create the 'Add Braces' Code Action
            var action = CodeAction.Create("Convert to Full Property", c => ConvertToPrivateFieldWithPropertyAsync(context.Document, propDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Document> ConvertToPrivateFieldWithPropertyAsync(Document document, PropertyDeclarationSyntax autoProperty, CancellationToken cancellationToken) {

            var type = autoProperty.Type.ToString();
            var propName = autoProperty.Identifier.Text;
            var fieldName = "_" + propName[0].ToString().ToLower() + propName.Substring(1);
            var isStatic = autoProperty.Modifiers.Any(s => s.Kind() == SyntaxKind.StaticKeyword);

            FieldDeclarationSyntax field;

            // http://roslynquoter.azurewebsites.net/

            if (autoProperty.Initializer != null) {
                field = FieldDeclaration(
                            VariableDeclaration(IdentifierName(type))
                            .WithVariables(
                            SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(Identifier(fieldName)).WithInitializer(EqualsValueClause(
                             LiteralExpression(
                                 SyntaxKind.NumericLiteralExpression,
                                 Literal(
                                     TriviaList(),
                                     autoProperty.Initializer.Value.ToString(),
                                     autoProperty.Initializer.Value.ToString(),
                                     TriviaList()))))
                )))
                .WithModifiers(
                    TokenList(
                        Token(
                            SyntaxKind.PrivateKeyword)))
                .WithSemicolonToken(
                    Token(
                        SyntaxKind.SemicolonToken));
            } else {
                field = FieldDeclaration(VariableDeclaration(
                       IdentifierName(type))
                   .WithVariables(
                       SingletonSeparatedList<VariableDeclaratorSyntax>(
                           VariableDeclarator(
                               Identifier(
                                   fieldName)))))
                .WithModifiers(
                    TokenList(
                        Token(
                            SyntaxKind.PrivateKeyword)))
                .WithSemicolonToken(
                    Token(
                        SyntaxKind.SemicolonToken));
            }

            if (isStatic) {
                field = field.WithModifiers(TokenList(
                        Token(
                            SyntaxKind.PrivateKeyword),
                        Token(
                            SyntaxKind.StaticKeyword)
                ));
            }

            var prop = PropertyDeclaration(
                IdentifierName(type),
                Identifier(propName))
            .WithModifiers(
                TokenList(autoProperty.Modifiers))
            .WithAccessorList(
                AccessorList(
                    List<AccessorDeclarationSyntax>(
                    new AccessorDeclarationSyntax[]{
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithBody(
                            Block(
                                SingletonList<StatementSyntax>(
                                    ReturnStatement(
                                        IdentifierName(fieldName))))),
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithBody(
                            Block(
                                SingletonList<StatementSyntax>(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName(fieldName),
                                            IdentifierName("value"))))))})));



            // Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken)
                                        .ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(autoProperty, new SyntaxNode[] {
                field,
                prop,
            });

            return document.WithSyntaxRoot(newRoot);
        }

    }
}