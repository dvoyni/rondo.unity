using UnityEngine;

namespace Rondo.Unity.Managed {
    public class HotReloadComponent : MonoBehaviour {
        private static HotReloadComponent _instance;

        public bool UseDumpedModel;

        public static HotReloadComponent Instance {
            get {
                if (!_instance) {
                    _instance = FindObjectOfType<HotReloadComponent>();
                }
                if (!_instance) {
                    _instance = new GameObject(
                        $"{nameof(Rondo)}.{nameof(HotReloadComponent)}",
                        typeof(HotReloadComponent)
                    ).GetComponent<HotReloadComponent>();
                }
                return _instance;
            }
        }
    }
}