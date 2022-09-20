using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct CanvasGroupConfig {
            public readonly float Alpha;
            public readonly bool Interactable;
            public readonly bool BlocksRaycasts;
            public readonly bool IgnoreParentGroups;

            public CanvasGroupConfig(
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
        }

        private static readonly ulong _idCanvasGroup = CompExtensions.NextId;

        public static Comp CanvasGroup(CanvasGroupConfig config) {
            return new Comp(_idCanvasGroup, &SyncCanvasGroup, Mem.C.CopyPtr(config));
        }

        private static void SyncCanvasGroup(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<CanvasGroup>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<CanvasGroup>();
            }

            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<CanvasGroupConfig>();
            var next = *pNext.Cast<CanvasGroupConfig>();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.Alpha != next.Alpha)) {
                canvasGroup.alpha = next.Alpha;
            }
            if (force || (prev.Interactable != next.Interactable)) {
                canvasGroup.interactable = next.Interactable;
            }
            if (force || (prev.BlocksRaycasts != next.BlocksRaycasts)) {
                canvasGroup.blocksRaycasts = next.BlocksRaycasts;
            }
            if (force || (prev.IgnoreParentGroups != next.IgnoreParentGroups)) {
                canvasGroup.ignoreParentGroups = next.IgnoreParentGroups;
            }
        }
    }
}