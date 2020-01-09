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

namespace ReflectionIT.Analyzer.Analyzers.AutoPropertiesInStructs {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertiesInStructsCodeFixProvider)), Shared]
    public class AutoPropertiesInStructsCodeFixProvider : CodeFixProvider {

        private const string Title = "Convert to Field";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AutoPropertiesInStructsAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertToFieldAsync(context.Document, root, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ConvertToFieldAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax property, CancellationToken cancellationToken) {
           
            var sm = await document.GetSemanticModelAsync();
            var ps = sm.GetDeclaredSymbol(property) as IPropertySymbol;

            IEnumerable<SyntaxToken> GetModifiers() {
                foreach (var item in property.Modifiers) {
                    yield return item;
                }
                if (ps.IsReadOnly) {
                    yield return SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword);
                }
            }

            var field = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(property.Type)
                                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(property.Identifier))))
                                        .WithModifiers(SyntaxFactory.TokenList(GetModifiers()));

            // Replace old with new
            var newRoot = root.ReplaceNode(property, field);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;

        }


    }
}