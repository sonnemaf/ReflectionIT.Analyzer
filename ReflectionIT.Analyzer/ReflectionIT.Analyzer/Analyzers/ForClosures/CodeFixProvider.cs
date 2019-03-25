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

namespace ReflectionIT.Analyzer.Analyzers.ForClosure {

    // Source: https://channel9.msdn.com/events/TechDays/Techdays-2016-The-Netherlands/Introduction-to-building-code-analyzers-and-code-fixes-with-Roslyn

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ForClosureCodeFixProvider)), Shared]
    public class ForClosureCodeFixProvider : CodeFixProvider {
        private const string Title = "Capture using local variable";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ForClosureAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var identifier = (IdentifierNameSyntax)root.FindToken(diagnosticSpan.Start).Parent;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => FixClosuresAsync(context.Document, identifier, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> FixClosuresAsync(Document document, IdentifierNameSyntax identifier, CancellationToken cancellationToken) {
            var sem = await document.GetSemanticModelAsync();

            var symbol = sem.GetSymbolInfo(identifier).Symbol;

            var forStatement = identifier.AncestorsAndSelf().OfType<ForStatementSyntax>().First(fss => fss.Declaration.Variables.Any(var => sem.GetDeclaredSymbol(var) == symbol));

            var newIdentifier = SyntaxFactory.IdentifierName(identifier.Identifier.Text + "Closure");

            var newStatement = (StatementSyntax)new Replacer(symbol, sem, newIdentifier).Visit(forStatement.Statement);

            var body = newStatement;
            if (!body.IsKind(SyntaxKind.Block)) {
                body = SyntaxFactory.Block(body);
            }

            var blockBody = (BlockSyntax)body;

            var closureVariableInitializer = SyntaxFactory
                .LocalDeclarationStatement(SyntaxFactory
                    .VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(SyntaxFactory
                        .SingletonSeparatedList(SyntaxFactory
                            .VariableDeclarator(newIdentifier.Identifier)
                            .WithInitializer(SyntaxFactory
                                .EqualsValueClause(identifier)
                            )
                        )
                    )
                )
                .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

            var newBlockBody = SyntaxFactory.Block(new[] { closureVariableInitializer }
                .Concat(blockBody.Statements));

            var newForStatement = forStatement.WithStatement(newBlockBody);

            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.ReplaceNode(forStatement, newForStatement);

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        class Replacer : CSharpSyntaxRewriter {
            private readonly SemanticModel _model;
            private readonly SyntaxNode _newIdentifier;
            private readonly ISymbol _symbol;

            public Replacer(ISymbol symbol, SemanticModel model, SyntaxNode newIdentifier) {
                _symbol = symbol;
                _model = model;
                _newIdentifier = newIdentifier;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
                var symbol = _model.GetSymbolInfo(node);
                return symbol.Symbol == _symbol ? _newIdentifier : base.VisitIdentifierName(node);
            }
        }
    }
}