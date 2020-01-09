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
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Editing;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {

    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class EmptyArrayCodeFixProvider : CodeFixProvider {
        private const string title = "Replace with Array.Empty<T>()";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(EmptyArrayAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) {

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => UseArrayEmptyAsync(c, context.Document, diagnosticSpan),
                    equivalenceKey: title),
                diagnostic);

            return Task.FromResult(false);
        }

        private async Task<Document> UseArrayEmptyAsync(CancellationToken cancellationToken, Document document, TextSpan diagnosticSpan) {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var arrayCreation = root.FindNode(diagnosticSpan);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var arrayOperation = (IArrayCreationOperation)semanticModel.GetOperation(arrayCreation);

            var generator = SyntaxGenerator.GetGenerator(document);

            // Get the type of the elements of the array (new int[] => int)
            var elementType = GetArrayElementType(arrayCreation, semanticModel, cancellationToken);
            if (elementType == null)
                return document;

            var arrayTypeExpr = generator.TypeExpression(semanticModel.Compilation.GetTypeByMetadataName("System.Array"));
            var genericExpr = generator.GenericName("Empty", elementType);
            var memberAccess = generator.MemberAccessExpression(arrayTypeExpr, genericExpr);
            var invocationExpression = generator.InvocationExpression(memberAccess);

            var newRoot = root.ReplaceNode(arrayCreation, invocationExpression);
            var newDoc = document.WithSyntaxRoot(newRoot);

            return newDoc;
        }

        private static ITypeSymbol GetArrayElementType(SyntaxNode arrayCreationExpression, SemanticModel semanticModel, CancellationToken cancellationToken) {
            var typeInfo = semanticModel.GetTypeInfo(arrayCreationExpression, cancellationToken);
            var arrayType = (IArrayTypeSymbol)(typeInfo.Type ?? typeInfo.ConvertedType);
            return arrayType?.ElementType;
        }

    }
}