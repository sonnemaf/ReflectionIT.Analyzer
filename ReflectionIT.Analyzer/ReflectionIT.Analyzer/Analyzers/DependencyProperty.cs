// https://github.com/tomlokhorst/codeanalysis-checkdependencyproperty/blob/master/src/CheckDependencyProperty/PropertyDiagnosticAnalyzer.cs
//using System;
//using System.Collections.Immutable;
//using System.Threading;
//using System.Linq;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace ReflectionIT.Analyzer.Analyzers {

//    [DiagnosticAnalyzer]
//    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
//    public class PropertyDiagnosticAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind> {
//        internal const string DiagnosticId = "PropertyDependencyProperty";
//        internal const string Category = "Conventions";

//        internal const string DiagnosticDescription = "DependencyProperty refers to different property";
//        internal const string DiagnosticMessageFormat = "DependencyProperty '{0}' refers to '{1}' instead of this property '{2}'";
//        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, DiagnosticDescription, DiagnosticMessageFormat, Category, DiagnosticSeverity.Warning);

//        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
//            get { return ImmutableArray.Create(Rule); }
//        }

//        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest {
//            get { return ImmutableArray.Create(SyntaxKind.PropertyDeclaration); }
//        }

//        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) {
//            var property = node as PropertyDeclarationSyntax;
//            if (property == null) return;

//            var classDecl = property.Parent as ClassDeclarationSyntax;
//            if (classDecl == null) return;

//            var accessors = property.AccessorList.Accessors;
//            var getter = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
//            var setter = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

//            if (getter != null) {
//                var identifier = identifierFromGetter(classDecl, getter, semanticModel);
//                var diagnostic = checkField(classDecl, property.Identifier.ValueText, identifier, semanticModel);
//                if (diagnostic != null)
//                    addDiagnostic(diagnostic);
//            }

//            if (setter != null) {
//                var identifier = identifierFromSetter(classDecl, setter, semanticModel);
//                var diagnostic = checkField(classDecl, property.Identifier.ValueText, identifier, semanticModel);
//                if (diagnostic != null)
//                    addDiagnostic(diagnostic);
//            }
//        }

//        private Diagnostic checkField(ClassDeclarationSyntax classDecl, string propertyName, IdentifierNameSyntax identifierArgument, SemanticModel semanticModel) {
//            if (identifierArgument == null) return null;
//            var fieldName = identifierArgument.Identifier.ValueText;

//            var fields =
//              from f in classDecl.Members.OfType<FieldDeclarationSyntax>()
//              where f.Declaration.Variables.Count > 0
//              let variable = f.Declaration.Variables[0]
//              where variable.Identifier.ValueText == fieldName
//              select f;

//            var field = fields.FirstOrDefault();
//            if (field == null) return null;

//            var registerArguments = FieldDiagnosticAnalyzer.GetArgumentList(field, semanticModel);
//            if (registerArguments == null) return null;

//            var nameLiteral = registerArguments.Arguments[0].Expression as LiteralExpressionSyntax;
//            if (nameLiteral == null || !nameLiteral.IsKind(SyntaxKind.StringLiteralExpression)) return null;
//            var nameLiteralValueText = nameLiteral.Token.ValueText;

//            if (nameLiteralValueText == propertyName) return null;

//            return Diagnostic.Create(Rule, identifierArgument.GetLocation(), ImmutableArray.Create(field.GetLocation()), fieldName, nameLiteralValueText, propertyName);
//        }

//        private IdentifierNameSyntax identifierFromGetter(ClassDeclarationSyntax classDecl, AccessorDeclarationSyntax accessor, SemanticModel semanticModel) {
//            if (accessor.Body == null) return null;

//            var statements = accessor.Body.Statements;
//            if (statements.Count != 1) return null;

//            var returnStatement = statements[0] as ReturnStatementSyntax;
//            if (returnStatement == null) return null;

//            var cast = returnStatement.Expression as CastExpressionSyntax;
//            if (cast == null) return null;

//            var invocation = cast.Expression as InvocationExpressionSyntax;
//            if (invocation == null) return null;

//            var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression).Symbol;
//            if (symbolInfo.ToDisplayString() != "Windows.UI.Xaml.DependencyObject.GetValue(Windows.UI.Xaml.DependencyProperty)") return null;

//            var arguments = invocation.ArgumentList.Arguments;
//            if (arguments.Count != 1) return null;

//            var identifierArgument = arguments[0].Expression as IdentifierNameSyntax;

//            return identifierArgument;
//        }

//        private IdentifierNameSyntax identifierFromSetter(ClassDeclarationSyntax classDecl, AccessorDeclarationSyntax accessor, SemanticModel semanticModel) {
//            if (accessor.Body == null) return null;

//            var statements = accessor.Body.Statements;
//            if (statements.Count != 1) return null;

//            var expressionStatement = statements[0] as ExpressionStatementSyntax;
//            if (expressionStatement == null) return null;

//            var invocation = expressionStatement.Expression as InvocationExpressionSyntax;
//            if (invocation == null) return null;

//            var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression).Symbol;
//            if (symbolInfo.ToDisplayString() != "Windows.UI.Xaml.DependencyObject.SetValue(Windows.UI.Xaml.DependencyProperty, object)") return null;

//            var arguments = invocation.ArgumentList.Arguments;
//            if (arguments.Count != 2) return null;

//            var identifierArgument = arguments[0].Expression as IdentifierNameSyntax;

//            return identifierArgument;
//        }
//    }



//}
