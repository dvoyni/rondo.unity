using System.Collections.Generic;
using Rondo.Core;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using Unity.Mathematics;

namespace Rondo.Unity.Cmds {
    public static unsafe class Tweener {
        //TODO: create tweener

        public static float EaseLinear(float x) => x;
        public static float EaseInQuad(float x) => x * x;
        public static float EaseOutQuad(float x) => 1 - (1 - x) * (1 - x);
        public static float EaseInOutQuad(float x) => x < 0.5 ? 2 * x * x : 1 - (-2 * x + 2) * (-2 * x + 2) / 2;
    }
}