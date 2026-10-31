using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Morpeh.SourceGenerators {
    [Generator(LanguageNames.CSharp)]
    public sealed class ComponentRegistrationGenerator : IIncrementalGenerator {
        private static readonly DiagnosticDescriptor Inaccessible = new DiagnosticDescriptor(
            "MORPEH001", "Component is inaccessible to generated registration",
            "'{0}' implements Morpeh.IComponent but is not accessible to generated registration; make it (and its containers) internal or public",
            "Morpeh", DiagnosticSeverity.Error, isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GenericComponent = new DiagnosticDescriptor(
            "MORPEH002", "Component cannot be registered as an open generic type",
            "'{0}' is generic or nested in a generic type and cannot be registered; move it to a non-generic type",
            "Morpeh", DiagnosticSeverity.Error, isEnabledByDefault: true);

        private readonly record struct Candidate(string Name, bool Accessible, bool Generic, int? CacheSize, bool HasDispose, LocationInfo? Location);

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var candidates = context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => node is StructDeclarationSyntax ||
                        node is RecordDeclarationSyntax record && record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword),
                    static (syntax, _) => Classify(syntax))
                .Where(static candidate => candidate.HasValue)
                .Select(static (candidate, _) => candidate!.Value)
                .Collect()
                .Select(static (items, _) => new EquatableArray<Candidate>(items
                    .GroupBy(candidate => candidate.Name, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .OrderBy(candidate => candidate.Name, StringComparer.Ordinal)
                    .ToImmutableArray()));
            var assembly = context.CompilationProvider.Select(static (compilation, _) => compilation.AssemblyName);
            context.RegisterSourceOutput(assembly.Combine(candidates), static (production, pair) => Emit(production, pair.Left, pair.Right.Array));
        }

        private static Candidate? Classify(GeneratorSyntaxContext context) {
            if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol type) {
                return null;
            }
            var component = context.SemanticModel.Compilation.GetTypeByMetadataName("Morpeh.IComponent");
            if (component is null || !type.AllInterfaces.Contains(component, SymbolEqualityComparer.Default)) {
                return null;
            }
            var generic = false;
            for (var container = type; container is not null; container = container.ContainingType) {
                generic |= container.Arity != 0;
            }
            var accessible = !generic && GenUtil.IsAccessibleToSiblingInAssembly(type);
            int? cacheSize = null;
            var cacheAttribute = context.SemanticModel.Compilation.GetTypeByMetadataName("Morpeh.CacheSizeAttribute");
            foreach (var attribute in type.GetAttributes()) {
                if (cacheAttribute is not null && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, cacheAttribute) &&
                    attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is int size) {
                    cacheSize = size;
                    break;
                }
            }
            var dispose = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable).GetMembers("Dispose").OfType<IMethodSymbol>().FirstOrDefault();
            var implementation = dispose is null ? null : type.FindImplementationForInterfaceMember(dispose);
            var hasDispose = implementation is not null && !SymbolEqualityComparer.Default.Equals(implementation.ContainingType, component);
            return new Candidate(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), accessible, generic, cacheSize, hasDispose,
                accessible ? null : LocationInfo.From(context.Node));
        }

        private static void Emit(SourceProductionContext context, string? assembly, ImmutableArray<Candidate> candidates) {
            if (candidates.IsDefaultOrEmpty) {
                return;
            }
            foreach (var candidate in candidates) {
                if (!candidate.Accessible) {
                    context.ReportDiagnostic(Diagnostic.Create(candidate.Generic ? GenericComponent : Inaccessible, candidate.Location?.ToLocation() ?? Location.None, candidate.Name));
                }
            }
            var components = candidates.Where(candidate => candidate.Accessible).ToArray();
            if (components.Length == 0) {
                return;
            }
            var name = "ComponentRegistrar_" + GenUtil.SafeIdentifier(assembly);
            var source = new StringBuilder();
            source.AppendLine("namespace Morpeh.Generated {");
            source.AppendLine("    [global::UnityEngine.Scripting.Preserve]");
            source.AppendLine($"    internal static class {name} {{");
            source.AppendLine("        [global::UnityEngine.Scripting.Preserve]");
            source.AppendLine("        [global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]");
            source.AppendLine("        private static void Register() {");
            foreach (var component in components) {
                var size = component.CacheSize?.ToString(CultureInfo.InvariantCulture);
                var arguments = component.HasDispose ? size ?? "" : size is null ? "hasDispose: false" : size + ", hasDispose: false";
                source.AppendLine($"            global::Morpeh.TypeIdentifier<{component.Name}>.Register({arguments});");
            }
            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine("}");
            context.AddSource(name + ".g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        }
    }
}
