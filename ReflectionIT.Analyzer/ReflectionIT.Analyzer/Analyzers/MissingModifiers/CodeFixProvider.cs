using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace ReflectionIT.Analyzer.Analyzers.NonPrivateField {


    // Source: VSDiagnostics -> ExplicitAccessModifiersCodeFix

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingModifiersCodeFixProvider)), Shared]
    public class MissingModifiersCodeFixProvider : CodeFixProvider {

        private const string _title = "Add modifier";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MissingModifiersAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var statement = root.FindNode(diagnosticSpan);

            var semanticModel = await context.Document.GetSemanticModelAsync();
            var symbol = semanticModel.GetDeclaredSymbol(statement);
            var accessibility = symbol?.DeclaredAccessibility ?? Accessibility.Private;

            if (symbol is not null && (symbol.Kind == SymbolKind.Method || symbol.Kind == SymbolKind.Property) && (symbol.IsVirtual || symbol.IsAbstract)) {
                accessibility = Accessibility.Protected;
            }

            context.RegisterCodeFix(
                CodeAction.Create(_title, 
                                  x => AddModifierAsync(context.Document, root, statement, accessibility), 
                                  _title), diagnostic);
        }

        private Task<Solution> AddModifierAsync(Document document, SyntaxNode root, SyntaxNode statement,
                                                Accessibility accessibility) {
            var generator = SyntaxGenerator.GetGenerator(document);
            var newStatement = generator.WithAccessibility(statement, accessibility);

            var newRoot = root.ReplaceNode(statement, newStatement);
            return Task.FromResult(document.WithSyntaxRoot(newRoot).Project.Solution);
        }

    }
}