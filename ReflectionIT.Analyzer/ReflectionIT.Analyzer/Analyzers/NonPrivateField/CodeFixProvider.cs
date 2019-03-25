using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace ReflectionIT.Analyzer.Analyzers.NonPrivateField {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NonPrivateFieldCodeFixProvider)), Shared]
    public class NonPrivateFieldCodeFixProvider : CodeFixProvider {

        private const string Title = "Convert to Auto Property";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NonPrivateFieldAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertToAutoPropertyAsync(context.Document, root, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        // public string Achternaam = "Sonnemans";

        private Task<Document> ConvertToAutoPropertyAsync(Document document, SyntaxNode root, FieldDeclarationSyntax field, CancellationToken cancellationToken) {
            var l = new List<PropertyDeclarationSyntax>();

            foreach (var variable in field.Declaration.Variables) {

                var identifierToken = variable.Identifier;

                var prop = SyntaxFactory.PropertyDeclaration(field.Declaration.Type, identifierToken)
                .WithModifiers(field.Modifiers)
                .WithAccessorList(SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]{
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithKeyword(
                            SyntaxFactory.Token(
                                SyntaxKind.GetKeyword))
                        .WithSemicolonToken(
                            SyntaxFactory.Token(
                                SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithKeyword(
                            SyntaxFactory.Token(
                                SyntaxKind.SetKeyword))
                        .WithSemicolonToken(
                            SyntaxFactory.Token(
                                SyntaxKind.SemicolonToken))}))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseBraceToken)));

                if (variable.Initializer != null) {
                    prop = prop.WithInitializer(variable.Initializer).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }

                l.Add(prop);
            }

            //// Replace old with new

            var newRoot = root.ReplaceNode(field, l);

            var newDocument = document.WithSyntaxRoot(newRoot);

            return Task.FromResult(newDocument);

        }


    }
}