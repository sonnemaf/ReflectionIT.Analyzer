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
    public class ReadonlyRefLocalOnNonReadonlyStructAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RAS0003";

        private static readonly LocalizableString _title = "Ref readonly local for a non-readonly value type (struct)";
        private static readonly LocalizableString _messageFormat = "Ref readonly local '{0}' is a non-readonly struct which may leads to defensive copies";
        private static readonly LocalizableString _description = "Ref readonly local should be a readonly value type (struct) or a reference type.";
        private const string _category = "Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement));
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;
            var cancellationToken = context.CancellationToken;

            if (syntaxNode is LocalDeclarationStatementSyntax localDeclaration) {

                foreach (var local in localDeclaration.Declaration.Variables) {

                    if (context.SemanticModel.GetDeclaredSymbol(local) is ILocalSymbol resolvedLocal) {
                        if (resolvedLocal.Type.IsValueType &&
                            !resolvedLocal.Type.IsReadOnly &&
                            resolvedLocal.IsRef &&
                            resolvedLocal.RefKind == RefKind.In) {

                            // This is ref readonly variable that is not friendly for in-refs.
                            var diagnostic = Diagnostic.Create(_rule, localDeclaration.Declaration.Type.GetLocation(), resolvedLocal.Type.Name, resolvedLocal.Name);

                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

    }
}
