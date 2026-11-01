using System.Collections.Generic;
using Nodes.Battle.Units;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Config {
    [CreateAssetMenu(menuName = "Config/Unit Preset")]
    public class UnitPreset : ScriptableObject {
        public UnitSideEnum Side;
        public AssetReferenceGameObject View;

        /// <summary>
        /// Applied on spawn. Base stats are Add stat modifiers
        /// </summary>
        public List<MutatorConfig> Mutators = new ();
    }
}
