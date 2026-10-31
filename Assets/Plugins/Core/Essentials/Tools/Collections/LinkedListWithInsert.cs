using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Tools.Collections {
    public class LinkedListWithInsert<T> : IEnumerable<T> where T : class {
        public int Count => _size;

        [NonSerialized] private int _size;
        [NonSerialized] private ListNode _root;
        [NonSerialized] private ListNode _last;

        public ListNode AddFirst(T obj) {
            Interlocked.Increment(ref _size);

            var node = new ListNode(this, obj);
            if (_root == null) {
                _root = _last = node;
                return node;
            }

            _root.Previous = node;
            node.Next = _root;
            _root = node;
            return node;
        }

        public ListNode AddLast(T obj) {
            Interlocked.Increment(ref _size);

            var node = new ListNode(this, obj);
            if (_root == null) {
                _root = _last = node;
                return node;
            }

            _last.Next = node;
            node.Previous = _last;
            _last = node;
            return node;
        }

        public bool Remove(ListNode node) => node.TryDispose();

        public bool Remove(T value) {
            var next = _root;
            while (next != null) {
                if (next.Value == value) {
                    next.Dispose();
                    return true;
                }

                next = next.Next;
            }

            return false;
        }

        public ListNode InsertAfter(T value, ListNode afterValue) {
            Interlocked.Increment(ref _size);
            
            var insertNode = new ListNode(this, value);
            
            if (_root == null) {
                _root = _last = insertNode;
                return insertNode;
            }

            if (afterValue == null || afterValue.IsDisposed) {
                afterValue = _last;
            }
            
            insertNode.Previous = afterValue;

            var right = afterValue.Next;
            afterValue.Next = insertNode;
                
            if (right != null) {
                insertNode.Next = right;
                right.Previous = insertNode;
                return insertNode;
            }
            
            _last = afterValue;
            return insertNode;
        }

        public ListNode InsertAfter(T value, T afterValue) {
            Interlocked.Increment(ref _size);

            var insertNode = new ListNode(this, value);
            
            if (_root == null) {
                _root = _last = insertNode;
                return insertNode;
            }

            var left = _root;
            if (afterValue != null) {
                while (left != null) {
                    if (left.Value == afterValue)
                        break;

                    left = left.Next;
                }
            }

            if (left == null) {
                _last.Next = insertNode;
                insertNode.Previous = _last;
                _last = insertNode;
                return insertNode;
            }

            var right = left.Next;
            if (right == null) {
                _last = insertNode;
            }
            else {
                right.Previous = insertNode;
                insertNode.Next = right;
            }

            left.Next = insertNode;
            insertNode.Previous = left;

            return insertNode;
        }

        private void UnsubscribeNode(ListNode wrapper) {
            Interlocked.Decrement(ref _size);

            if (wrapper == _root) {
                _root = wrapper.Next;
            }

            if (wrapper == _last) {
                _last = wrapper.Previous;
            }

            if (wrapper.Previous != null) {
                wrapper.Previous.Next = wrapper.Next;
            }

            if (wrapper.Next != null) {
                wrapper.Next.Previous = wrapper.Previous;
            }
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator {
            private LinkedListWithInsert<T> _list;
            private bool _iterating;
            private ListNode _current;

            internal Enumerator(LinkedListWithInsert<T> list) {
                _list = list;
                _current = null;
                _iterating = false;
            }

            public void Dispose() { }

            public bool MoveNext() {
                if (_iterating == false) {
                    _iterating = true;
                    _current = _list._root;
                }
                else {
                    _current = _current.Next;
                }

                return _current != null;
            }

            public T Current => _current?.Value;

            System.Object System.Collections.IEnumerator.Current => _current?.Value;

            void System.Collections.IEnumerator.Reset() {
                _iterating = false;
                _current = null;
            }
        }

        public class ListNode : IDisposable {
            public T Value { get; }
            private LinkedListWithInsert<T> _list;

            public ListNode Previous { get; internal set; }
            public ListNode Next { get; internal set; }
            
            public bool IsDisposed => _list == null;

            public ListNode(LinkedListWithInsert<T> list, T value) {
                this._list = list;
                this.Value = value;
            }

            public bool TryDispose() {
                if (IsDisposed) {
                    return false;
                }

                Dispose();
                return true;
            }

            public void Dispose() {
                var sourceList = Interlocked.Exchange(ref _list, null);
                if (sourceList != null) {
                    sourceList.UnsubscribeNode(this);
                    sourceList = null;
                }
                Previous = null;
                Next = null;
            }
        }
    }
}