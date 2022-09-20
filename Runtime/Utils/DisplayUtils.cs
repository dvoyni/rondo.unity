using Rondo.Unity.Subs;
using Unity.Mathematics;

namespace Rondo.Unity.Utils {
    public static class DisplayUtils {
        public static float3x2 ScreenPointToRay(Display.CameraMx mx, float2 resolution, float2 point) {
            var imx = math.inverse(math.mul(mx.Projection, mx.WorldToCamera));
            var pos = math.mul(imx, new float4(point.x / resolution.x * 2 - 1, point.y / resolution.y * 2 - 1, 0.0f, 1));
            var dir = math.mul(imx, new float4(0, 0, -1, 0));

            return new float3x2(pos.xyz * 1.0f / pos.w, math.normalize(dir.xyz));
        }
    }
}