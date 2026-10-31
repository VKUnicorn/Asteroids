using Morpeh.Collections;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        internal int[] typeIds;
        internal bool isDirty;
        internal BitMap entitiesBitMap;
        internal FastList<Filter> filters;
        internal UnsafeIntHashMap<int> removeTransfer;
        internal UnsafeIntHashMap<int> addTransfer;
        internal int length;
        internal int id;

        internal World world;

        internal Archetype(int id, int[] typeIds, World world) {
            this.id = id;
            this.typeIds = typeIds;
            this.length = 0;
            this.entitiesBitMap = new BitMap();
            this.addTransfer = new UnsafeIntHashMap<int>();
            this.removeTransfer = new UnsafeIntHashMap<int>();

            this.isDirty = false;
            this.world = world;

            this.Ctor();
        }
    }
}