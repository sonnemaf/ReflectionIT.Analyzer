//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace ReflectionIT.Analyzer.Structs {

//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class NonReadonlyMemberAccessAnalyzer : DiagnosticAnalyzer {
//        public const string DiagnosticId = "RAS0004";

//        private static readonly LocalizableString _title = "non readonly member access TODO"; 
//        private static readonly LocalizableString _messageFormat = "non readonly member access '{0}' is a non-readonly struct which may leads to defensive copies TODO";
//        private static readonly LocalizableString _description = "non readonly member access TODO";

//        private const string Category = "Usage";

//        private static DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

//        public override void Initialize(AnalysisContext context) {
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            context.RegisterSyntaxNodeAction(AnalyzeNode, ImmutableArray.Create(SyntaxKind.SimpleMemberAccessExpression));
//        }

//        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
//            var syntaxNode = context.Node;
//            var semanticModel = context.SemanticModel;
//            var cancellationToken = context.CancellationToken;
//            if (syntaxNode is MemberAccessExpressionSyntax ma) {
//                var parameterSymbol = semanticModel.GetDeclaredSymbol(ma, cancellationToken);

//                // Test for an InParameter with a Non-Readonly ValueType
//                //if (ma != null &&
//                //    ma.Modifiers.Any(m => m.IsKind(SyntaxKind.InKeyword)) &&
//                //    parameterSymbol.Type.IsValueType &&
//                //    !parameterSymbol.Type.IsReadOnly) {

//                //    // Create the Code Diagnostic
//                //    var diagnostic = Diagnostic.Create(_rule, ma.GetLocation(), ma.Identifier.Text.ToString());

//                //    context.ReportDiagnostic(diagnostic);
//                //}
//            }
//        }
//    }
   
//}
