using System.Collections.Generic;
using UnityEngine;

namespace Config {
    [CreateAssetMenu(menuName = "Config/Units Database")]
    public class UnitsDatabase : ScriptableObject {
        public List<UnitPreset> Units = new ();
    }
}
