using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Morpeh.Collections {
    public sealed class UnrolledList<T> : IDisposable {
        private int _count;
        private T[][] _nodes;
        private readonly int _nodeSize;
        public int Count => _count;

        public T this[int index] {
            get => GetNodeFor(index, out var elementIndex)[elementIndex];
            set => GetNodeFor(index, out var elementIndex)[elementIndex] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index) => ref GetNodeFor(index, out var elementIndex)[elementIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T[] GetNodeFor(int index, out int elementIndex) {
            var nodeIndex = Math.DivRem(index, _nodeSize, out elementIndex);
            return _nodes[nodeIndex];
        }

        public UnrolledList(int nodeSize = 64) {
            _nodeSize = Math.Max(4, nodeSize);
            _nodes = Array.Empty<T[]>();
        }

        public void AllocateForCount(int size) {
            if (size <= _count) {
                return;
            }
            var nodesCount = Math.DivRem(size, _nodeSize, out var rem) + (rem > 0 ? 1 : 0);
            _count = nodesCount * _nodeSize;
            if (_nodes.Length < nodesCount) {
                Array.Resize(ref _nodes, nodesCount);
                for (int i = _nodes.Length - 1; i >= 0; i--) {
                    if (_nodes[i] == null) {
                        _nodes[i] = ArrayPool<T>.Shared.Rent(_nodeSize);
                    }
                    else {
                        break;
                    }
                }
            }
        }

        public void Clear() {
            foreach (var node in _nodes) {
                ArrayPool<T>.Shared.Return(node);
            }

            _count = 0; 
            _nodes = Array.Empty<T[]>();
        }

        public void ClearArrays() {
            foreach (var node in _nodes) {
                Array.Clear(node, 0, node.Length);
            }
        }

        public void Dispose() {
            Clear();
        }
    }
}