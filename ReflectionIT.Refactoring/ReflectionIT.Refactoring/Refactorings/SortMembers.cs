using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using ReflectionIT.Analyzer.Helpers;

namespace ReflectionIT.Analyzer.Refactorings {
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SortMembers)), Shared]
    public class SortMembers : CodeRefactoringProvider {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a ClassDeclarationSyntax
            var clsDecl = node as ClassDeclarationSyntax;
            if (clsDecl == null) {
                return;
            }

            // Create the 'Add Braces' Code Action
            var action = CodeAction.Create("Sort Members", c => SortMembersAsync(context.Document, clsDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Document> SortMembersAsync(Document document, ClassDeclarationSyntax clsDecl, CancellationToken cancellationToken) {

            // Sort Fields
            var fieldsOld = clsDecl.ChildNodes().OfType<FieldDeclarationSyntax>().ToArray();
            var fieldsNew = fieldsOld.OrderBy(m => m.Declaration.Variables.First().Identifier.Value).ToArray();

            // Sort Properties
            var allProperties = clsDecl.ChildNodes().OfType<PropertyDeclarationSyntax>().ToArray();

            var propOld = allProperties.Where(p => p.ExpressionBody != null || p.AccessorList.Accessors.All(a => a.ChildNodes().OfType<BlockSyntax>().Any())).ToArray();
            var propNew = propOld.OrderBy(m => m.Identifier.Value).ToArray();

            var autoPropOld = allProperties.Except(propNew).ToArray();
            var autoPropNew = autoPropOld.OrderBy(m => m.Identifier.Value).ToArray();

            // Sort Events
            var eventsOld = clsDecl.ChildNodes().OfType<EventFieldDeclarationSyntax>().ToArray();
            var eventsNew = eventsOld.OrderBy(m => m.Declaration.Variables.First().Identifier.Value).ToArray();

            // Sort Constructors
            var ctorsOld = clsDecl.ChildNodes().OfType<ConstructorDeclarationSyntax>().ToArray();
            var ctorsNew = ctorsOld.OrderBy(m => m.ParameterList.Parameters.Count).ToArray();

            // Sort Methods
            var methodsOld = clsDecl.ChildNodes().OfType<MethodDeclarationSyntax>().ToArray();
            var methodsNew = methodsOld.OrderBy(m => m.Identifier.Value).ThenBy(m => m.ParameterList.Parameters.Count).ToArray();

            // Append members
            var oldMembers = new List<MemberDeclarationSyntax>();
            oldMembers.AddRange(fieldsOld);
            oldMembers.AddRange(autoPropOld);
            oldMembers.AddRange(eventsOld);
            oldMembers.AddRange(ctorsOld);
            oldMembers.AddRange(propOld);
            oldMembers.AddRange(methodsOld);

            var newMembers = new List<MemberDeclarationSyntax>();
            newMembers.AddRange(fieldsNew);
            newMembers.AddRange(autoPropNew);
            newMembers.AddRange(eventsNew);
            newMembers.AddRange(ctorsNew);
            newMembers.AddRange(propNew);
            newMembers.AddRange(methodsNew);

            // Replace old with new
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken)
                                        .ConfigureAwait(false);

            var index = 0;
            var newRoot = oldRoot.ReplaceNodes(oldMembers,
                (o, n) => newMembers[index++]
            );

            return document.WithSyntaxRoot(newRoot);
        }

    }
}