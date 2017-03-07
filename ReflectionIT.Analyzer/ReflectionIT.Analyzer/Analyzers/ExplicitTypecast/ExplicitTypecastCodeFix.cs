using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
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

namespace ReflectionIT.Analyzer.Analyzers.ExplicitTypecast {

    /// <summary>
    /// int x = o; => int x = (int)o;
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitTypecastCodeFix)), Shared]
    public class ExplicitTypecastCodeFix : CodeFixProvider {
        
        public const string DiagnosticId = "CS0266"; 

        private const string title = "Add explicit typecast";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed async override Task RegisterCodeFixesAsync(CodeFixContext context) {

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().FirstOrDefault();

            if (declaration == null || declaration.Variables.Count > 1) {
                // TODO, ook oplossen
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => AddExplicitTypcastAsync(context.Document, root, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> AddExplicitTypcastAsync(Document document, SyntaxNode root, VariableDeclarationSyntax declaration, CancellationToken c) {
            var d = declaration.Variables.First();
            var i = d.Initializer;
            var cast = SyntaxFactory.CastExpression((TypeSyntax)declaration.Type, i.Value);

            // Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(c)
                                        .ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(i.Value, cast);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}