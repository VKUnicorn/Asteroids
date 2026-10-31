#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using JetBrains.Annotations;
using Morpeh.Collections;
using Morpeh;
using System.Runtime.CompilerServices;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public static class FilterExtensions {
        internal static void Dispose(this Filter filter) {
            foreach (var child in filter.childs) {
                child.Dispose();
            }

            filter.childs.Clear();
            filter.childs = null;

            if (filter.archetypes != null) {
                foreach (var archetype in filter.archetypes) {
                    archetype.RemoveFilter(filter);
                }

                filter.archetypes.Clear();
                filter.archetypes = null;
            }

            filter.includedTypeIds?.Clear();
            filter.includedTypeIds = null;
            filter.excludedTypeIds?.Clear();
            filter.excludedTypeIds = null;

            filter.Length = -1;

            filter.typeID = -1;
            filter.mode = Filter.Mode.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void UpdateLength(this Filter filter) {
            filter.isDirty = false;
            filter.Length = 0;
            foreach (var archetype in filter.archetypes) {
                filter.Length += archetype.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindArchetypes(this Filter filter, IntFastList newArchetypes) {
            var minLength = filter.includedTypeIds.length;
            foreach (var archId in newArchetypes) {
                var arch = filter.world.archetypes.data[archId];
                filter.CheckArchetype(arch, minLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindArchetypes(this Filter filter) {
            var minLength = filter.includedTypeIds.length;
            foreach (var arch in filter.world.archetypes) {
                filter.CheckArchetype(arch, minLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckArchetype(this Filter filter, Archetype archetype, int minLength) {
            var typeIdsLength = archetype.typeIds.Length;
            if (typeIdsLength >= minLength) {
                var check = true;
                for (int i = 0, length = minLength; i < length; i++) {
                    var includedTypeId = filter.includedTypeIds.Get(i);
                    var foundInclude = false;
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > includedTypeId) {
                            check = false;
                            goto BREAK;
                        }

                        if (typeId == includedTypeId) {
                            foundInclude = true;
                            break;
                        }
                    }

                    if (foundInclude == false) {
                        check = false;
                        goto BREAK;
                    }
                }

                for (int i = 0, length = filter.excludedTypeIds.length; i < length; i++) {
                    var excludedTypeId = filter.excludedTypeIds.Get(i);
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > excludedTypeId) {
                            break;
                        }

                        if (typeId == excludedTypeId) {
                            check = false;
                            goto BREAK;
                        }
                    }
                }

                BREAK:
                if (check) {
                    for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                        if (filter.archetypes.data[i] == archetype) {
                            return;
                        }
                    }

                    filter.archetypes.Add(archetype);
                    archetype.AddFilter(filter);
                }
            }
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this Filter filter, in int id) {
            var enumerator = filter.GetEnumerator();
            for (int i = 0, length = id + 1; i < length; i++) {
                if (enumerator.MoveNext() == false) {
                    throw new IndexOutOfRangeException();
                }
            }
            return enumerator.Current;
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity First(this Filter filter) {
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FirstOrDefault(this Filter filter) {
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }

            return default;
        }

        public static Filter With<T>(this Filter filter) where T : struct, IComponent => filter.CreateFilter<T>(Filter.Mode.Include);
        public static Filter Without<T>(this Filter filter) where T : struct, IComponent => filter.CreateFilter<T>(Filter.Mode.Exclude);
        public static Filter With(this Filter filter, Type type) => filter.CreateFilter(type, Filter.Mode.Include);
        public static Filter Without(this Filter filter, Type type) => filter.CreateFilter(type, Filter.Mode.Exclude);

        private static Filter CreateFilter(this Filter filter, Type type, Filter.Mode mode) => CreateFilter(filter, CommonTypeIdentifier.typeAssociation[type].id, mode);
        private static Filter CreateFilter<T>(this Filter filter, Filter.Mode mode) where T : struct, IComponent => CreateFilter(filter, TypeIdentifier<T>.Info.id, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Filter CreateFilter(this Filter filter, int typeId, Filter.Mode mode) {
            for (int i = 0, length = filter.childs.length; i < length; i++) {
                var child = filter.childs.data[i];
                if (child.mode == mode && child.typeID == typeId) {
                    return child;
                }
            }

            IntFastList newIncludedTypeIds;
            IntFastList newExcludedTypeIds;
            if (filter.typeID == -1) {
                newIncludedTypeIds = new IntFastList();
                newExcludedTypeIds = new IntFastList();
            }
            else {
                newIncludedTypeIds = new IntFastList(filter.includedTypeIds);
                newExcludedTypeIds = new IntFastList(filter.excludedTypeIds);
            }

            if (mode == Filter.Mode.Include) {
                newIncludedTypeIds.Add(typeId);
            }
            else if (mode == Filter.Mode.Exclude) {
                newExcludedTypeIds.Add(typeId);
            }

            var newFilter = new Filter(filter.world, typeId, newIncludedTypeIds, newExcludedTypeIds, mode);
            filter.childs.Add(newFilter);

            return newFilter;
        }
    }
}