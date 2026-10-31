using System;
using System.Collections.Generic;

namespace Core.Tools.Collections {
    public class SortedListWithDuplicates<TKey, TValue> : SortedList<TKey, TValue> where TKey : IComparable {
        public SortedListWithDuplicates() : base(new DuplicateKeyComparer<TKey>()) {}

        public SortedListWithDuplicates(int capacity) : base(capacity, new DuplicateKeyComparer<TKey>()) {}

        private class DuplicateKeyComparer<TDuplicateKey> : IComparer<TDuplicateKey> where TDuplicateKey : IComparable {
            public int Compare(TDuplicateKey left, TDuplicateKey right) {
                var result = left.CompareTo(right);
                // Handle equality as being greater. Note: this will break Remove(key) or
                // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result == 0 ? 1 : result;
            }
        }
    }
}