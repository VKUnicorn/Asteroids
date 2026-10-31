using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Core.Tools.Extensions {
    public static class DictionaryExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static V GetOrCreateDefault<K, V>(this Dictionary<K, V> dictionary, K key) where V : class, new() {
            if (dictionary.TryGetValue(key, out var v)) {
                return v;
            }

            v = new V();
            dictionary.Add(key, v);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static V Pop<K, V>(this Dictionary<K, V> dictionary, K key) {
            if (dictionary.TryGetValue(key, out var value)) {
                dictionary.Remove(key);
                return value;
            }

            return default(V);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push<K, V>(this Dictionary<K, V> dictionary, K key, V value) {
            dictionary[key] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<K, C> ConvertValues<K, V, C>(this IDictionary<K, V> dict, Func<K, V, C> converter) {
            var result = new Dictionary<K, C>();
            foreach (var pair in dict) {
                result[pair.Key] = converter(pair.Key, pair.Value);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAndRemove<K, V>(this Dictionary<K, V> dictionary, K key, out V value) {
            if (dictionary.TryGetValue(key, out value)) {
                dictionary.Remove(key);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (K, V)[] ToTupleArray<K, V>(this Dictionary<K, V> dictionary) {
            var result = new (K, V)[dictionary.Count];
            var index = 0;
            foreach (var pair in dictionary) {
                result[index++] = (pair.Key, pair.Value);
            }

            return result;
        }
    }
}