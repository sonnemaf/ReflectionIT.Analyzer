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

using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.Editing;

namespace ReflectionIT.Analyzer.Analyzers.ExplicitTypecast {

    /// <summary>
    /// int x = o; => int x = (int)o;
    /// return o; => return (int)o;
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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();

            if (declaration != null) {

                var varDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().FirstOrDefault();


                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => AddExplicitTypcastAsync(context.Document, declaration, varDeclaration, c),
                        equivalenceKey: title),
                    diagnostic);


            }
        }



        private async Task<Document> AddExplicitTypcastAsync(Document document, VariableDeclaratorSyntax declaration, VariableDeclarationSyntax varDeclaration, CancellationToken c) {
            var d = declaration;
            var i = d.Initializer;

            //var cast = SyntaxGenerator.GetGenerator(document).CastExpression(varDeclaration.Type, i.Value).NormalizeWhitespace();

            var cast = SyntaxFactory.CastExpression(varDeclaration.Type, i.Value).NormalizeWhitespace();

            /// Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(i.Value, cast);

            return document.WithSyntaxRoot(newRoot);
        }



        
    }



}
