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
    public class ReadonlyFieldOnNonReadonlyStructAnalyzer : DiagnosticAnalyzer {
        public const string DiagnosticId = "RAS0002";

        private static readonly LocalizableString _title = "Readonly field for a non-readonly struct";
        private static readonly LocalizableString _messageFormat = "Readonly field '{0}' is a non-readonly struct which may leads to defensive copies";
        private static readonly LocalizableString _description = "Readonly fields should be a readonly value type or a reference type.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            // Find readonly fields with Non-Readonly ValueType
            if (fieldSymbol.IsReadOnly &&
                fieldSymbol.Type.IsValueType &&
                !fieldSymbol.Type.IsReadOnly) {

                //&& !parameterSymbol.Type.ContainingNamespace.Name.StartsWith("System")

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rule, fieldSymbol.Locations[0], fieldSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        //public ref readonly int Test() {

        //}

    }
}
