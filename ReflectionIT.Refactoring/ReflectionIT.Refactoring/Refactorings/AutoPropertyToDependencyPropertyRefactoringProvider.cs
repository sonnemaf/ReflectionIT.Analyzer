

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
//using ReflectionIT.Analyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReflectionIT.Analyzer.Refactorings {

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToDependencyPropertyRefactoringProvider)), Shared]
    public class AutoPropertyToDependencyPropertyRefactoringProvider : CodeRefactoringProvider {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a PropertyDeclarationSyntax
            if (!(node is PropertyDeclarationSyntax propDecl)) {
                return;
            }

            var isStatic = propDecl.Modifiers.Any(s => s.Kind() == SyntaxKind.StaticKeyword);
            if (isStatic) {
                return;
            }

            if (!(propDecl.Parent is ClassDeclarationSyntax cls)) {
                return;
            }

            var baseCls = cls.BaseList?.Types.FirstOrDefault();
            if (baseCls == null) {
                return;
            }

            // Only offer a refactoring if the selected node is a AUTO property with a getter AND a setter
            if (propDecl.AccessorList == null || propDecl.AccessorList.Accessors.All(a => a.ChildNodes().OfType<BlockSyntax>().Any()) || propDecl.AccessorList.Accessors.Count != 2) {
                return;
            }

            // Create the 'Add Braces' Code Action
            var action = CodeAction.Create("Convert to Dependency Property", c => ConvertToDependencyPropertyAsync(context.Document, propDecl, cls, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }


        private async Task<Document> ConvertToDependencyPropertyAsync(Document document, PropertyDeclarationSyntax autoProperty, ClassDeclarationSyntax cls, CancellationToken cancellationToken) {

            var type = autoProperty.Type.ToString();
            var propName = autoProperty.Identifier.Text;
            var propNameProperty = $"{propName}Property";
            var propNamePropertyChanged = $"On{propName}PropertyChanged";
            var clsName = cls.Identifier.ToString();
            var defaultValue = autoProperty.Initializer != null ? autoProperty.Initializer.Value.ToString() : "null";

            // http://roslynquoter.azurewebsites.net/

            var members = new MemberDeclarationSyntax[]{
            SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.IdentifierName(type),
                SyntaxFactory.Identifier(propName))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(
                        SyntaxKind.PublicKeyword)))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]{
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.CastExpression(
                                                SyntaxFactory.IdentifierName(type),
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.IdentifierName(
                                                        @"GetValue"))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(propNameProperty))))
                                                    .WithOpenParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.OpenParenToken))
                                                    .WithCloseParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.CloseParenToken))))
                                            .WithOpenParenToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.OpenParenToken))
                                            .WithCloseParenToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.CloseParenToken)))
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
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.IdentifierName(
                                                    @"SetValue"))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]{
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(propNameProperty)),
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(
                                                                    @"value"))}))
                                                .WithOpenParenToken(
                                                    SyntaxFactory.Token(
                                                        SyntaxKind.OpenParenToken))
                                                .WithCloseParenToken(
                                                    SyntaxFactory.Token(
                                                        SyntaxKind.CloseParenToken))))
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
                                    SyntaxKind.SetKeyword))}))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseBraceToken))),
            SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName(
                        @"DependencyProperty"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(propNameProperty))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(
                                            @"DependencyProperty"),
                                        SyntaxFactory.IdentifierName(
                                            @"Register"))
                                    .WithOperatorToken(
                                        SyntaxFactory.Token(
                                            SyntaxKind.DotToken)))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.IdentifierName(
                                                            @"nameof"))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.IdentifierName(propName))))
                                                        .WithOpenParenToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.OpenParenToken))
                                                        .WithCloseParenToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.CloseParenToken)))),
                                                SyntaxFactory.Token(
                                                    SyntaxKind.CommaToken),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.TypeOfExpression(
                                                        SyntaxFactory.IdentifierName(type))
                                                    .WithKeyword(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.TypeOfKeyword))
                                                    .WithOpenParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.OpenParenToken))
                                                    .WithCloseParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.CloseParenToken))),
                                                SyntaxFactory.Token(
                                                    SyntaxKind.CommaToken),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.TypeOfExpression(
                                                        SyntaxFactory.IdentifierName(clsName))
                                                    .WithKeyword(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.TypeOfKeyword))
                                                    .WithOpenParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.OpenParenToken))
                                                    .WithCloseParenToken(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.CloseParenToken))),
                                                SyntaxFactory.Token(
                                                    SyntaxKind.CommaToken),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.IdentifierName(
                                                            @"PropertyMetadata"))
                                                    .WithNewKeyword(
                                                        SyntaxFactory.Token(
                                                            SyntaxKind.NewKeyword))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    SyntaxFactory.Argument(
                                                                        autoProperty.Initializer == null ?
                                                                        SyntaxFactory.ObjectCreationExpression(
                                                                            SyntaxFactory.IdentifierName(type))
                                                                        .WithNewKeyword(
                                                                            SyntaxFactory.Token(
                                                                                SyntaxKind.NewKeyword))
                                                                        .WithArgumentList(
                                                                            SyntaxFactory.ArgumentList()
                                                                            .WithOpenParenToken(
                                                                                SyntaxFactory.Token(
                                                                                    SyntaxKind.OpenParenToken))
                                                                            .WithCloseParenToken(
                                                                                SyntaxFactory.Token(
                                                                                    SyntaxKind.CloseParenToken))) as ExpressionSyntax
                                                                                    :
                                                                        SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.NumericLiteralExpression,
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(),
                                                                            autoProperty.Initializer.Value.ToString(),
                                                                            autoProperty.Initializer.Value.ToString(),
                                                                            SyntaxFactory.TriviaList()))),
                                                                    SyntaxFactory.Token(
                                                                        SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName(propNamePropertyChanged))}))
                                                        .WithOpenParenToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.OpenParenToken))
                                                        .WithCloseParenToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.CloseParenToken))))}))
                                    .WithOpenParenToken(
                                        SyntaxFactory.Token(
                                            SyntaxKind.OpenParenToken))
                                    .WithCloseParenToken(
                                        SyntaxFactory.Token(
                                            SyntaxKind.CloseParenToken))))
                            .WithEqualsToken(
                                SyntaxFactory.Token(
                                    SyntaxKind.EqualsToken))))))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    new []{
                        SyntaxFactory.Token(
                            SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(
                            SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(
                            SyntaxKind.ReadOnlyKeyword)}))
            .WithSemicolonToken(
                SyntaxFactory.Token(
                    SyntaxKind.SemicolonToken)),
            SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(
                        SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(propNamePropertyChanged))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    new []{
                        SyntaxFactory.Token(
                            SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(
                            SyntaxKind.StaticKeyword)}))
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]{
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(
                                    @"d"))
                            .WithType(
                                SyntaxFactory.IdentifierName(
                                    @"DependencyObject")),
                            SyntaxFactory.Token(
                                SyntaxKind.CommaToken),
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(
                                    @"e"))
                            .WithType(
                                SyntaxFactory.IdentifierName(
                                    @"DependencyPropertyChangedEventArgs"))}))
                .WithOpenParenToken(
                    SyntaxFactory.Token(
                        SyntaxKind.OpenParenToken))
                .WithCloseParenToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseParenToken)))
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.List<StatementSyntax>(
                        new StatementSyntax[]{
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.IdentifierName(
                                        @"var"))
                                .WithVariables(
                                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(
                                                @"source"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.AsExpression,
                                                    SyntaxFactory.IdentifierName(
                                                        @"d"),
                                                    SyntaxFactory.IdentifierName(clsName))
                                                .WithOperatorToken(
                                                    SyntaxFactory.Token(
                                                        SyntaxKind.AsKeyword)))
                                            .WithEqualsToken(
                                                SyntaxFactory.Token(
                                                    SyntaxKind.EqualsToken))))))
                            .WithSemicolonToken(
                                SyntaxFactory.Token(
                                    SyntaxKind.SemicolonToken)),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    SyntaxFactory.IdentifierName(
                                        @"source"),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)
                                    .WithToken(
                                        SyntaxFactory.Token(
                                            SyntaxKind.NullKeyword)))
                                .WithOperatorToken(
                                    SyntaxFactory.Token(
                                        SyntaxKind.ExclamationEqualsToken)),
                                SyntaxFactory.Block(
                                    //SyntaxFactory.SingletonList<StatementSyntax>(
                                    //    SyntaxFactory.Token(SyntaxKind.SingleLineCommentTrivia)),
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(
                                                SyntaxFactory.IdentifierName(
                                                    @"var"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                    SyntaxFactory.VariableDeclarator(
                                                        SyntaxFactory.Identifier(
                                                            @"value"))
                                                    .WithInitializer(
                                                        SyntaxFactory.EqualsValueClause(
                                                            SyntaxFactory.CastExpression(
                                                                SyntaxFactory.IdentifierName(type),
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(
                                                                        @"e"),
                                                                    SyntaxFactory.IdentifierName(
                                                                        @"NewValue"))
                                                                .WithOperatorToken(
                                                                    SyntaxFactory.Token(
                                                                        SyntaxKind.DotToken)))
                                                            .WithOpenParenToken(
                                                                SyntaxFactory.Token(
                                                                    SyntaxKind.OpenParenToken))
                                                            .WithCloseParenToken(
                                                                SyntaxFactory.Token(
                                                                    SyntaxKind.CloseParenToken)))
                                                        .WithEqualsToken(
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.EqualsToken))))))
                                .WithSemicolonToken(
                                            SyntaxFactory.Token(
                                                SyntaxFactory.TriviaList(),
                                                SyntaxKind.SemicolonToken,
                                                SyntaxFactory.TriviaList(
                                                    SyntaxFactory.Comment(
                                                        @" // TODO: Handle new value."))))))
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
                                    SyntaxKind.CloseParenToken))}))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseBraceToken)))};

            // Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken)
                                        .ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(autoProperty, members);

            return document.WithSyntaxRoot(newRoot);
        }

    }
}
