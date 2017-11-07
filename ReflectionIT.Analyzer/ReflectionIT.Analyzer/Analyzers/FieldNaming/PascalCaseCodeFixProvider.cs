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

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PascalCaseCodeFixProvider)), Shared]
    public class PascalCaseCodeFixProvider : CodeFixProvider {
        private const string title = "Make PascalCase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FieldNameAnalyzer.DiagnosticIdPascalName); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => MakePascalCaseAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> MakePascalCaseAsync(Document document, VariableDeclaratorSyntax field, CancellationToken cancellationToken) {
            var identifierToken = field.Identifier;

            var newName = FieldNameAnalyzer.GetCorrectPascalName(identifierToken.Text);

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(identifierToken.Parent, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}