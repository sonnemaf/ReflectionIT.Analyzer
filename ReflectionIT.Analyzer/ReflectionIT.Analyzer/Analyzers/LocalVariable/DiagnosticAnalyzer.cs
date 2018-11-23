using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionIT.Analyzer.Analyzers.PrivateField {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalVariableAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "RIT0006";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.LocalVariableAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.LocalVariableAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.LocalVariableAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.Naming;

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context) {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            var localVar = (LocalDeclarationStatementSyntax)context.Node;
            foreach (var item in localVar.Declaration.Variables) {

                var name = item.Identifier.Text;

                // Find locals (not constants) with invalid names
                if (!localVar.IsConst &&
                    !string.IsNullOrWhiteSpace(name) &&
                    !char.IsLower(name[0])) {

                    var newName = name[0].ToString().ToLower() + name.Substring(1);

                    if (newName != name) {

                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(_rule, item.GetLocation(), name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

   
}
