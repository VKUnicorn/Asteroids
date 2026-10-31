using System;
using System.Runtime.CompilerServices;

namespace Core.Tools.Extensions {
    public static class StringExtensions {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Or(this string value, string fallback) => string.IsNullOrEmpty(value) ? fallback : value;
        
        /// <summary>
        /// Returns whether the specified string is contained with this string
        /// Credits to JaredPar http://stackoverflow.com/questions/444798/case-insensitive-containsstring/444818#444818
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        
        /// <summary>
        /// Returns true if this string is null or empty
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str) {
            return string.IsNullOrEmpty(str);
        }
    }
}