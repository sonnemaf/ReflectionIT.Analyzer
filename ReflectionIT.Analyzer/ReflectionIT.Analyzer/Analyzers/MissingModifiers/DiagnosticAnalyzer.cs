using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReflectionIT.Analyzer.Helpers;

namespace ReflectionIT.Analyzer.Analyzers.NonPrivateField {

    // Source: VSDiagnostics -> ExplicitAccessModifiersAnalyzer

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingModifiersAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0004";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.MissingModifiersAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.MissingModifiersAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.MissingModifiersAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.PracticesAndImprovements;

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        private readonly SyntaxKind[] _accessModifierKinds =
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
            SyntaxKind.PrivateKeyword
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(HandleClass, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(HandleConstructor, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(HandleDelegate, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(HandleEnum, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(HandleEvent, SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(HandleEventField, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleField, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleIndexer, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(HandleInterface, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(HandleMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleProperty, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(HandleStruct, SyntaxKind.StructDeclaration);
        }

        private void HandleClass(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (ClassDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleStruct(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (StructDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleEnum(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (EnumDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleDelegate(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (DelegateDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleInterface(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (InterfaceDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleField(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (FieldDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.GetLocation(), declarationExpression.Declaration.Variables.First().Identifier.Value));
                //context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.GetLocation(), "private"));
            }
        }

        private void HandleProperty(SyntaxNodeAnalysisContext context) {
            if (context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)) {
                return;
            }

            var declarationExpression = (PropertyDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds) &&
                declarationExpression.ExplicitInterfaceSpecifier == null) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleMethod(SyntaxNodeAnalysisContext context) {
            if (context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)) {
                return;
            }

            var declarationExpression = (MethodDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds) &&
                !declarationExpression.Modifiers.Contains(SyntaxKind.PartialKeyword) &&
                declarationExpression.ExplicitInterfaceSpecifier == null) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleConstructor(SyntaxNodeAnalysisContext context) {
            var declarationExpression = (ConstructorDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds) &&
                !declarationExpression.Modifiers.Contains(SyntaxKind.StaticKeyword)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    "constructor"));
            }
        }

        private void HandleEventField(SyntaxNodeAnalysisContext context) {
            if (context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)) {
                return;
            }

            var declarationExpression = (EventFieldDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.GetLocation(), declarationExpression.Declaration.Variables.First().Identifier.Value));
            }
        }

        private void HandleEvent(SyntaxNodeAnalysisContext context) {
            if (context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)) {
                return;
            }

            var declarationExpression = (EventDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds)) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.Identifier.GetLocation(),
                    declarationExpression.Identifier.Text));
            }
        }

        private void HandleIndexer(SyntaxNodeAnalysisContext context) {
            if (context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)) {
                return;
            }

            var declarationExpression = (IndexerDeclarationSyntax)context.Node;
            if (!declarationExpression.Modifiers.ContainsAny(_accessModifierKinds) &&
                declarationExpression.ExplicitInterfaceSpecifier == null) {
                var accessibility = context.SemanticModel.GetDeclaredSymbol(declarationExpression).DeclaredAccessibility;
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarationExpression.GetLocation(),
                    "indexer"));
            }
        }

    }

}
