using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Structs {

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RefReadonlyParameterOnNonReadonlyStructAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RAS0004";

        private static readonly LocalizableString _title = "Ref readonly parameter on a non-readonly struct"; 
        private static readonly LocalizableString _messageFormat = "Ref readonly parameter '{0}' is a non-readonly struct which may leads to defensive copies";
        private static readonly LocalizableString _description = "Ref readonly parameters should be a readonly struct or a reference type";

        private const string _category = "Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, ImmutableArray.Create(SyntaxKind.Parameter));
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;
            var cancellationToken = context.CancellationToken;
            if (syntaxNode is ParameterSyntax parameterNode) {
                var parameterSymbol = semanticModel.GetDeclaredSymbol(parameterNode, cancellationToken);

                // Test for an InParameter with a Non-Readonly ValueType
                if (parameterNode != null &&
                    parameterNode.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)) &&
                    parameterSymbol.Type.IsValueType &&
                    !parameterSymbol.Type.IsReadOnly) {

                    // Create the Code Diagnostic
                    var diagnostic = Diagnostic.Create(_rule, parameterNode.GetLocation(), parameterNode.Identifier.Text.ToString());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
   
}
