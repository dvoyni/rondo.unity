using Rondo.Unity.Utils;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static  partial class UI {
        public readonly struct CanvasGroup : IComp {
            public readonly float Alpha;
            public readonly bool Interactable;
            public readonly bool BlocksRaycasts;
            public readonly bool IgnoreParentGroups;

            public CanvasGroup(
                float alpha = 1,
                bool interactable = true,
                bool blocksRaycasts = true,
                bool ignoreParentGroups = false
            ) {
                Alpha = alpha;
                Interactable = interactable;
                BlocksRaycasts = blocksRaycasts;
                IgnoreParentGroups = ignoreParentGroups;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var canvasGroup = create ? gameObject.AddComponent<UnityEngine.CanvasGroup>() : gameObject.GetComponent<UnityEngine.CanvasGroup>();
                var prev = create ? default : (CanvasGroup)cPrev;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.Alpha != Alpha)) {
                    canvasGroup.alpha = Alpha;
                }
                if (create || (prev.Interactable != Interactable)) {
                    canvasGroup.interactable = Interactable;
                }
                if (create || (prev.BlocksRaycasts != BlocksRaycasts)) {
                    canvasGroup.blocksRaycasts = BlocksRaycasts;
                }
                if (create || (prev.IgnoreParentGroups != IgnoreParentGroups)) {
                    canvasGroup.ignoreParentGroups = IgnoreParentGroups;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {

                Helpers.DestroySafe<UnityEngine.CanvasGroup>(gameObject);
            }
        }
    }
}