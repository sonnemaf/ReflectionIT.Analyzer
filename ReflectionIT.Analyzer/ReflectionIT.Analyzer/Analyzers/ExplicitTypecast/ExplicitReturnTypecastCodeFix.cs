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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitTypecastCodeFix2)), Shared]
    public class ExplicitTypecastCodeFix2 : CodeFixProvider {

        public const string DiagnosticId = "CS0266";

        private const string title = "Add explicit typecast";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

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



            // Find the type assignment identified by the diagnostic.
            var assignment = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null) {

                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => AddExplicitTypcastAsync(context.Document, assignment, c),
                        equivalenceKey: title),
                    diagnostic);

            } else {
                // Find the type declaration identified by the diagnostic.
                var returnStatement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ReturnStatementSyntax>().FirstOrDefault();
                if (returnStatement != null) {

                    TypeSyntax returnType = null;

                    var property = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                    if (property != null) {
                        returnType = property.Type;
                    } else {
                        var method = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                        if (method != null) {
                            returnType = method.ReturnType;
                        }
                    }

                    if (returnType != null) {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: title,
                                createChangedDocument: c => AddExplicitTypcastAsync(context.Document, returnStatement, returnType, c),
                                equivalenceKey: title),
                            diagnostic);
                    }

                }
            }

        }


        private async Task<Document> AddExplicitTypcastAsync(Document document, ReturnStatementSyntax returnStatement, TypeSyntax returnType, CancellationToken c) {
            var expr = returnStatement.Expression;
            var cast = SyntaxFactory.CastExpression(returnType, expr).NormalizeWhitespace();

            /// Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(expr, cast);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> AddExplicitTypcastAsync(Document document, AssignmentExpressionSyntax assignment, CancellationToken c) {

            var sem = await document.GetSemanticModelAsync();
            var ti = sem.GetTypeInfo(assignment.Left);

            var cast = SyntaxGenerator.GetGenerator(document).CastExpression(ti.Type, assignment.Right).NormalizeWhitespace();

            // Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);

            var newRoot = oldRoot.ReplaceNode(assignment.Right, cast);

            return document.WithSyntaxRoot(newRoot);
        }


    }



}
