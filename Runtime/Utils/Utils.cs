using UnityEngine;

namespace Rondo.Unity.Utils {
    internal static class Utils {
        public static void DestroySafe<T>(GameObject obj) where T : Component {
            var c = obj.GetComponent<T>();
            if (c) {
                Object.Destroy(c);
            }
        }
    }
}