using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct GraphicsRaycaster : IComp {
            public readonly bool IgnoreReversedGraphics;
            public readonly BlockingObjects BlockingObjects;
            public readonly int BlockingMask;

            public GraphicsRaycaster(
                BlockingObjects blockingObjects,
                bool ignoreReversedGraphics = true,
                int blockingMask = 0
            ) {
                IgnoreReversedGraphics = ignoreReversedGraphics;
                BlockingObjects = blockingObjects;
                BlockingMask = blockingMask;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var raycaster = create
                        ? gameObject.AddComponent<GraphicRaycaster>()
                        : gameObject.GetComponent<GraphicRaycaster>();
                var prev = create ? default : (GraphicsRaycaster)cPrev;

                if (create || (prev.IgnoreReversedGraphics != IgnoreReversedGraphics)) {
                    raycaster.ignoreReversedGraphics = IgnoreReversedGraphics;
                }

                if (create || (prev.BlockingObjects != BlockingObjects)) {
                    raycaster.blockingObjects = (GraphicRaycaster.BlockingObjects)BlockingObjects;
                }

                if (create || (prev.BlockingMask != BlockingMask)) {
                    raycaster.blockingMask = BlockingMask;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Utils.Helpers.DestroySafe<GraphicRaycaster>(gameObject);
            }
        }

        public enum BlockingObjects {
            None = 0,
            TwoD = 1,
            ThreeD = 2,
            All = 3,
        }
    }
}