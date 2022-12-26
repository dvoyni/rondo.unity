using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Utils {
    public class Colors {
        public static float4 Clear => new(0, 0, 0, 0);
        public static float4 White => new(1, 1, 1, 1);
        public static float4 Black => new(0, 0, 0, 1);

        internal static Color FromFloat4(float4 v) {
            return new Color(v.x, v.y, v.z, v.w);
        }
    }
}