using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Morpeh.SourceGenerators.Tests {
    public class ComponentRegistrationGeneratorTests {
        private const string Stubs = @"
namespace Morpeh {
    public interface IComponent : System.IDisposable { void System.IDisposable.Dispose() { } }
    [System.AttributeUsage(System.AttributeTargets.Struct)]
    public sealed class CacheSizeAttribute : System.Attribute { public CacheSizeAttribute(int size) { } }
    public static class TypeIdentifier<T> where T : struct, IComponent {
        public static void Register() { }
        public static void Register(int cacheSize) { }
        public static void Register(bool hasDispose) { }
        public static void Register(int cacheSize, bool hasDispose) { }
    }

}
namespace UnityEngine {
    public enum RuntimeInitializeLoadType { SubsystemRegistration }
    public class RuntimeInitializeOnLoadMethodAttribute : System.Attribute {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { }
    }
}
namespace UnityEngine.Scripting { public class PreserveAttribute : System.Attribute { } }
namespace System.Runtime.CompilerServices { public class IsExternalInit { } }
";
        private static readonly MetadataReference[] References = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
            .ToArray();

        private static (string Source, ImmutableArray<Diagnostic> Diagnostics, Compilation Output) Run(string source) {
            var options = new CSharpParseOptions(LanguageVersion.CSharp10);
            var compilation = CSharpCompilation.Create("WW.Main-Tests",
                new[] { CSharpSyntaxTree.ParseText(Stubs, options), CSharpSyntaxTree.ParseText(source, options, "Components.cs") },
                References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            CSharpGeneratorDriver.Create(new[] { new ComponentRegistrationGenerator().AsSourceGenerator() }, parseOptions: options)
                .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
            Assert.Empty(output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
            return (string.Concat(output.SyntaxTrees.Skip(2).Select(tree => tree.ToString())), diagnostics, output);
        }

        [Fact]
        public void Public_component_registers() {
            var result = Run("public struct Top : Morpeh.IComponent { }");
            Assert.Empty(result.Diagnostics);
            Assert.Contains("ComponentRegistrar_WW_Main_Tests", result.Source);
            Assert.Contains("global::Morpeh.TypeIdentifier<global::Top>.Register(hasDispose: false);", result.Source);
            Assert.DoesNotContain("PrewarmCaches", result.Source);
            Assert.DoesNotContain("RegisterCachePrewarm", result.Source);
            Assert.Contains("[global::UnityEngine.Scripting.Preserve]", result.Source);
        }

        [Theory]
        [InlineData("public", "public")]
        [InlineData("internal", "public")]
        [InlineData("public", "internal")]
        [InlineData("public", "protected internal")]
        public void Accessible_nested_component_registers(string containerAccess, string componentAccess) {
            var result = Run($"{containerAccess} class Outer {{ {componentAccess} struct Inner : Morpeh.IComponent {{ }} }}");
            Assert.Empty(result.Diagnostics);
            Assert.Contains("TypeIdentifier<global::Outer.Inner>.Register(hasDispose: false)", result.Source);
        }

        [Theory]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("private protected")]
        public void Inaccessible_nested_component_errors_and_is_skipped(string access) {
            var result = Run($"public class Outer {{ {access} struct Inner : Morpeh.IComponent {{ }} }}");
            Assert.Equal(string.Empty, result.Source);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal("MORPEH001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            Assert.Equal("Components.cs", diagnostic.Location.GetLineSpan().Path);
            Assert.Contains(access + " struct Inner", result.Output.SyntaxTrees.ElementAt(1).GetText().ToString(diagnostic.Location.SourceSpan));
        }

        [Fact]
        public void Inaccessible_container_errors_and_skips_public_component() {
            var result = Run("public class Outer { private class Hidden { public struct Component : Morpeh.IComponent { } } }");
            Assert.Equal(string.Empty, result.Source);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal("MORPEH001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        }

        [Theory]
        [InlineData("public struct Generic<T> : Morpeh.IComponent { }")]
        [InlineData("public class Outer<T> { public struct Nested : Morpeh.IComponent { } }")]
        [InlineData("public class Outer<T> { public class Inner { public struct Nested : Morpeh.IComponent { } } }")]
        public void Generic_component_or_container_errors(string source) {
            var result = Run(source + " public struct Visible : Morpeh.IComponent { }");
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal("MORPEH002", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            Assert.Equal("Components.cs", diagnostic.Location.GetLineSpan().Path);
            Assert.Contains("move it to a non-generic type", diagnostic.GetMessage());
            Assert.Contains("TypeIdentifier<global::Visible>.Register(hasDispose: false);", result.Source);
            Assert.DoesNotContain("<T>", result.Source);
        }

        [Fact]
        public void Class_and_non_component_are_skipped() {
            var result = Run("public class ComponentClass : Morpeh.IComponent { } public struct Other<T> { }");
            Assert.Empty(result.Diagnostics);
            Assert.Equal(string.Empty, result.Source);
        }

        [Fact]
        public void Transitive_interface_component_registers() {
            var result = Run("public interface IFoo : Morpeh.IComponent { } public struct Foo : IFoo { }");
            Assert.Contains("TypeIdentifier<global::Foo>.Register(hasDispose: false)", result.Source);
        }

        [Theory]
        [InlineData("[Morpeh.CacheSize(256)]", "256, hasDispose: false")]
        [InlineData("", "hasDispose: false")]
        public void Cache_size_matches_attribute_or_default(string attribute, string argument) {
            var result = Run($"{attribute} public struct Component : Morpeh.IComponent {{ }}");
            Assert.Contains($"TypeIdentifier<global::Component>.Register({argument});", result.Source);
        }

        [Fact]
        public void Record_struct_with_auto_property_registers() {
            var result = Run("public record struct Component(int Value) : Morpeh.IComponent;");
            Assert.Contains("TypeIdentifier<global::Component>.Register(hasDispose: false)", result.Source);
        }

        [Fact]
        public void Partial_declarations_register_once_with_complete_metadata() {
            var result = Run("public partial struct Component : Morpeh.IComponent { } public partial struct Component { private int _value; }");
            Assert.Equal(1, result.Source.Split(new[] { ".Register(" }, StringSplitOptions.None).Length - 1);
            Assert.Contains("Register(hasDispose: false)", result.Source);
        }

        [Fact]
        public void Output_is_deterministic_and_ordinal_sorted() {
            const string first = "public struct Z : Morpeh.IComponent { } public struct A : Morpeh.IComponent { }";
            const string second = "public struct A : Morpeh.IComponent { } public struct Z : Morpeh.IComponent { }";
            var generated = Run(first).Source;
            Assert.Equal(generated, Run(first).Source);
            Assert.Equal(generated, Run(second).Source);
            Assert.True(generated.IndexOf("<global::A>", StringComparison.Ordinal) < generated.IndexOf("<global::Z>", StringComparison.Ordinal));
        }

        [Fact]
        public void Automatic_registration_attribute_is_always_present() {
            var result = Run("public struct Component : Morpeh.IComponent { }");
            Assert.Contains("[global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]", result.Source);
            Assert.DoesNotContain("#if", result.Source);
            var registrar = result.Output.GetTypeByMetadataName("Morpeh.Generated.ComponentRegistrar_WW_Main_Tests")!;
            var register = registrar.GetMembers("Register").Single();
            Assert.Contains(register.GetAttributes(), attribute => attribute.AttributeClass!.Name == "RuntimeInitializeOnLoadMethodAttribute");
        }

        [Fact]
        public void Inaccessible_component_errors_while_public_component_registers() {
            var result = Run("public class Outer { private struct Hidden : Morpeh.IComponent { } } public struct Visible : Morpeh.IComponent { }");
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal("MORPEH001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            Assert.Contains("make it (and its containers) internal or public", diagnostic.GetMessage());
            Assert.Equal("Components.cs", diagnostic.Location.GetLineSpan().Path);
            Assert.Contains("TypeIdentifier<global::Visible>.Register(hasDispose: false);", result.Source);
            Assert.DoesNotContain("TypeIdentifier<global::Outer.Hidden>", result.Source);
        }

        [Theory]
        [InlineData("public struct Component : Morpeh.IComponent { public void Dispose() { } }", "Register();")]
        [InlineData("public struct Component : Morpeh.IComponent { void System.IDisposable.Dispose() { } }", "Register();")]
        [InlineData("[Morpeh.CacheSize(32)] public struct Component : Morpeh.IComponent { public void Dispose() { } }", "Register(32);")]
        [InlineData("public struct Component : Morpeh.IComponent { public void Dispose(bool all) { } }", "Register(hasDispose: false);")]
        [InlineData("public interface IOwn : Morpeh.IComponent { void System.IDisposable.Dispose() { } } public struct Component : IOwn { }", "Register();")]
        public void Own_dispose_keeps_dispose_call(string source, string registration) {
            Assert.Contains($"TypeIdentifier<global::Component>.{registration}", Run(source).Source);
        }

        [Fact]
        public void Compilation_without_components_emits_nothing() {
            Assert.Equal(string.Empty, Run(string.Empty).Source);
        }
    }
}
