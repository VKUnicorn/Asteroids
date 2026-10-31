using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Morpeh.SourceGenerators {
    internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>> where T : IEquatable<T> {
        public readonly ImmutableArray<T> Array;

        public EquatableArray(ImmutableArray<T> array) => Array = array;

        public bool Equals(EquatableArray<T> other) {
            if (Array.IsDefault) {
                return other.Array.IsDefault;
            }
            if (other.Array.IsDefault || Array.Length != other.Array.Length) {
                return false;
            }
            for (var i = 0; i < Array.Length; i++) {
                if (!Array[i].Equals(other.Array[i])) {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

        public override int GetHashCode() {
            if (Array.IsDefault) {
                return 0;
            }
            var hash = 17;
            foreach (var item in Array) {
                hash = hash * 31 + item.GetHashCode();
            }
            return hash;
        }
    }

    internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan) {
        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

        public static LocationInfo From(SyntaxNode node) {
            var location = node.GetLocation();
            return new LocationInfo(location.SourceTree!.FilePath, location.SourceSpan, location.GetLineSpan().Span);
        }
    }

    internal static class GenUtil {
        public static string SafeIdentifier(string? assemblyName) {
            if (string.IsNullOrEmpty(assemblyName)) {
                return "Unknown";
            }
            var sb = new StringBuilder(assemblyName!.Length);
            foreach (var ch in assemblyName) {
                sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
            }
            return sb.ToString();
        }

        public static bool IsAccessibleToSiblingInAssembly(INamedTypeSymbol type) {
            for (INamedTypeSymbol? t = type; t is not null; t = t.ContainingType) {
                if (t.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)) {
                    return false;
                }
            }
            return true;
        }
    }
}

namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit { }
}
