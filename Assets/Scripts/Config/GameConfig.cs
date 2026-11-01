using UnityEngine;

namespace Config {
    [CreateAssetMenu(menuName = "Config/Game Config")]
    public class GameConfig : ScriptableObject, IGameConfig {
        [SerializeField] private UnitsDatabase _unitsDatabase;

        public UnitsDatabase UnitsDatabase => _unitsDatabase;
    }
}
