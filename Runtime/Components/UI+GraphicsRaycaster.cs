using Rondo.Core.Memory;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public enum BlockingObjects {
            None = 0,
            TwoD = 1,
            ThreeD = 2,
            All = 3,
        }

        public readonly struct GraphicsRaycasterConfig {
            public readonly bool IgnoreReversedGraphics;
            public readonly BlockingObjects BlockingObjects;
            public readonly int BlockingMask;

            public GraphicsRaycasterConfig(
                BlockingObjects blockingObjects,
                bool ignoreReversedGraphics = true,
                int blockingMask = 0
            ) {
                IgnoreReversedGraphics = ignoreReversedGraphics;
                BlockingObjects = blockingObjects;
                BlockingMask = blockingMask;
            }
        }

        private static readonly ulong _idGraphicsRaycaster = CompExtensions.NextId;

        public static Comp GraphicsRaycaster(GraphicsRaycasterConfig config) {
            return new Comp(_idGraphicsRaycaster, &SyncGraphicsRaycaster, Mem.C.CopyPtr(config));
        }

        private static void SyncGraphicsRaycaster(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<GraphicRaycaster>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            var raycaster = gameObject.GetComponent<GraphicRaycaster>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<GraphicsRaycasterConfig>();
            var next = *pNext.Cast<GraphicsRaycasterConfig>();

            if (force || (prev.IgnoreReversedGraphics != next.IgnoreReversedGraphics)) {
                raycaster.ignoreReversedGraphics = next.IgnoreReversedGraphics;
            }

            if (force || (prev.BlockingObjects != next.BlockingObjects)) {
                raycaster.blockingObjects = (GraphicRaycaster.BlockingObjects)next.BlockingObjects;
            }

            if (force || (prev.BlockingMask != next.BlockingMask)) {
                raycaster.blockingMask = next.BlockingMask;
            }
        }
    }
}