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
    public class FieldNameAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticIdPrivateField = "RIT0001";
        public const string DiagnosticIdPascalName = "RIT0000";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString _titlePrivateField = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormatPrivateField = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _descriptionPrivateField = new LocalizableResourceString(nameof(Resources.PrivateFieldAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString _titleNonPrivateField = new LocalizableResourceString(nameof(Resources.NonPrivateFieldNameAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormatNonPrivateField = new LocalizableResourceString(nameof(Resources.NonPrivateFieldNameAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _descriptionNonPrivateField = new LocalizableResourceString(nameof(Resources.NonPrivateFieldNameAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = DiagnosticAnalyzerCategories.Naming;

        private static readonly DiagnosticDescriptor _rulePrivate = new DiagnosticDescriptor(DiagnosticIdPrivateField, _titlePrivateField, _messageFormatPrivateField, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _descriptionPrivateField);
        private static readonly DiagnosticDescriptor _ruleNonPrivate = new DiagnosticDescriptor(DiagnosticIdPascalName, _titleNonPrivateField, _messageFormatNonPrivateField, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _descriptionNonPrivateField);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rulePrivate, _ruleNonPrivate); } }

        public override void Initialize(AnalysisContext context) {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            // Find fields (not constants) with invalid names
            if (fieldSymbol.DeclaredAccessibility == Accessibility.Private &&
                !fieldSymbol.IsConst && !string.IsNullOrWhiteSpace(fieldSymbol.Name) &&
                fieldSymbol.Name != GetCorrectPrivateFieldName(fieldSymbol.Name)) {

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_rulePrivate, fieldSymbol.Locations[0], fieldSymbol.Name);

                context.ReportDiagnostic(diagnostic);

            }

            if (fieldSymbol.DeclaredAccessibility != Accessibility.Private &&
               !fieldSymbol.IsConst && !string.IsNullOrWhiteSpace(fieldSymbol.Name) &&
               fieldSymbol.Name != GetCorrectPascalName(fieldSymbol.Name)) {

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(_ruleNonPrivate, fieldSymbol.Locations[0], fieldSymbol.Name);

                context.ReportDiagnostic(diagnostic);

            }
        }

        internal static string GetCorrectPrivateFieldName(string name) {

            if (name.StartsWith("m_") && name.Length > 2) {
                name = "_" + name[2].ToString().ToLower() + name.Substring(3);
            } else {
                if (name.StartsWith("_") && name.Length > 1) {
                    name = "_" + name[1].ToString().ToLower() + name.Substring(2);
                } else {
                    if (name.Length > 0) {
                        name = "_" + name[0].ToString().ToLower() + name.Substring(1);
                    }
                }
            }

            return name;
        }

        internal static string GetCorrectPascalName(string name) {

            if (name.StartsWith("_")) {
                name = name.Substring(1);
            }
            if (name.Length > 0) {
                name = name[0].ToString().ToUpper() + name.Substring(1);
            }

            return name;
        }
    }
}
