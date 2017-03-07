using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using ReflectionIT.Analyzer.Helpers;

namespace ReflectionIT.Analyzer.Refactorings {
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged)), Shared]
    public class AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged : CodeRefactoringProvider {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a PropertyDeclarationSyntax
            var propDecl = node as PropertyDeclarationSyntax;
            if (propDecl == null) {
                return;
            }

            // Only offer a refactoring if the selected node is a AUTO property
            if (propDecl.AccessorList == null || propDecl.AccessorList.Accessors.All(a => a.ChildNodes().OfType<BlockSyntax>().Any()) || propDecl.AccessorList.Accessors.Count != 2) {
                return;
            }

            // Create the 'Add Braces' Code Action
            var action = CodeAction.Create("Convert to private field With Property and OnPropertyChanged()", c => ConvertToPrivateFieldWithPropertyAsync(context.Document, propDecl, c));

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
                field = SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(type))
                            .WithVariables(
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName)).WithInitializer(SyntaxFactory.EqualsValueClause(
                             SyntaxFactory.LiteralExpression(
                                 SyntaxKind.NumericLiteralExpression,
                                 SyntaxFactory.Literal(
                                     SyntaxFactory.TriviaList(),
                                     autoProperty.Initializer.Value.ToString(),
                                     autoProperty.Initializer.Value.ToString(),
                                     SyntaxFactory.TriviaList()))))
                )))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(
                            SyntaxKind.PrivateKeyword)))
                .WithSemicolonToken(
                    SyntaxFactory.Token(
                        SyntaxKind.SemicolonToken));
            } else {
                field = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(
                       SyntaxFactory.IdentifierName(type))
                   .WithVariables(
                       SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                           SyntaxFactory.VariableDeclarator(
                               SyntaxFactory.Identifier(
                                   fieldName)))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(
                            SyntaxKind.PrivateKeyword)))
                .WithSemicolonToken(
                    SyntaxFactory.Token(
                        SyntaxKind.SemicolonToken));
            }

            if (isStatic) {
                field = field.WithModifiers(SyntaxFactory.TokenList(
                        SyntaxFactory.Token(
                            SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(
                            SyntaxKind.StaticKeyword)
                ));
            }

            var prop = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.IdentifierName(type),
                SyntaxFactory.Identifier(propName))
            .WithModifiers(
                SyntaxFactory.TokenList(autoProperty.Modifiers))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]{
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.IdentifierName(fieldName))
                                        .WithReturnKeyword(
                                            SyntaxFactory.Token(
                                                SyntaxKind.ReturnKeyword))
                                        .WithSemicolonToken(
                                            SyntaxFactory.Token(
                                                SyntaxKind.SemicolonToken))))
                                .WithOpenBraceToken(
                                    SyntaxFactory.Token(
                                        SyntaxKind.OpenBraceToken))
                                .WithCloseBraceToken(
                                    SyntaxFactory.Token(
                                        SyntaxKind.CloseBraceToken)))
                            .WithKeyword(
                                SyntaxFactory.Token(
                                    SyntaxKind.GetKeyword)),
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.SetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.NotEqualsExpression,
                                                SyntaxFactory.IdentifierName(
                                                    @"value"),
                                                SyntaxFactory.IdentifierName(fieldName))
                                            .WithOperatorToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.ExclamationEqualsToken)),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.List<StatementSyntax>(
                                                    new StatementSyntax[]{
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.AssignmentExpression(
                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                SyntaxFactory.IdentifierName(fieldName),
                                                                SyntaxFactory.IdentifierName(
                                                                    @"value"))
                                                            .WithOperatorToken(
                                                                SyntaxFactory.Token(
                                                                    SyntaxKind.EqualsToken)))
                                                        .WithSemicolonToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.SemicolonToken)),
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.IdentifierName(
                                                                    @"OnPropertyChanged"))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.InvocationExpression(
                                                                                SyntaxFactory.IdentifierName(
                                                                                    @"nameof"))
                                                                            .WithArgumentList(
                                                                                SyntaxFactory.ArgumentList(
                                                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                                        SyntaxFactory.Argument(
                                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                                SyntaxFactory.ThisExpression()
                                                                                                .WithToken(
                                                                                                    SyntaxFactory.Token(
                                                                                                        SyntaxKind.ThisKeyword)),
                                                                                                SyntaxFactory.IdentifierName(propName))
                                                                                            .WithOperatorToken(
                                                                                                SyntaxFactory.Token(
                                                                                                    SyntaxKind.DotToken)))))
                                                                                .WithOpenParenToken(
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.OpenParenToken))
                                                                                .WithCloseParenToken(
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.CloseParenToken))))))
                                                                .WithOpenParenToken(
                                                                    SyntaxFactory.Token(
                                                                        SyntaxKind.OpenParenToken))
                                                                .WithCloseParenToken(
                                                                    SyntaxFactory.Token(
                                                                        SyntaxKind.CloseParenToken))))
                                                        .WithSemicolonToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.SemicolonToken))}))
                                            .WithOpenBraceToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.OpenBraceToken))
                                            .WithCloseBraceToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.CloseBraceToken)))
                                        .WithIfKeyword(
                                            SyntaxFactory.Token(
                                                SyntaxKind.IfKeyword))
                                        .WithOpenParenToken(
                                            SyntaxFactory.Token(
                                                SyntaxKind.OpenParenToken))
                                        .WithCloseParenToken(
                                            SyntaxFactory.Token(
                                                SyntaxKind.CloseParenToken))))
                                .WithOpenBraceToken(
                                    SyntaxFactory.Token(
                                        SyntaxKind.OpenBraceToken))
                                .WithCloseBraceToken(
                                    SyntaxFactory.Token(
                                        SyntaxKind.CloseBraceToken)))
                            .WithModifiers(autoProperty.AccessorList.Accessors.FirstOrDefault(a => a.Kind()== SyntaxKind.SetAccessorDeclaration).Modifiers)
                            .WithKeyword(
                                SyntaxFactory.Token(
                                    SyntaxKind.SetKeyword))}))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseBraceToken)));



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