using Rondo.Core.Memory;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        private static readonly ulong _idVerticalLayoutGroup = CompExtensions.NextId;

        public static Comp VerticalLayoutGroup(LayoutGroupConfig config) {
            return new Comp(_idVerticalLayoutGroup, &SyncVerticalLayoutGroup, Mem.C.CopyPtr(config));
        }

        private static void SyncVerticalLayoutGroup(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<VerticalLayoutGroup>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<VerticalLayoutGroup>();
            }

            var group = gameObject.GetComponent<VerticalLayoutGroup>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<LayoutGroupConfig>();
            var next = *pNext.Cast<LayoutGroupConfig>();

            if (force || !prev.Padding.Equals(next.Padding)) {
                group.padding = new RectOffset(next.Padding.w, next.Padding.y, next.Padding.x, next.Padding.z);
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.Spacing != next.Spacing)) {
                group.spacing = next.Spacing;
            }
            if (force || (prev.ChildAlignment != next.ChildAlignment)) {
                group.childAlignment = (UnityEngine.TextAnchor)next.ChildAlignment;
            }
            if (force || (prev.ReverseArrangement != next.ReverseArrangement)) {
                group.reverseArrangement = next.ReverseArrangement;
            }
            if (force || !prev.ChildControl.Equals(next.ChildControl)) {
                group.childControlWidth = next.ChildControl.x;
                group.childControlHeight = next.ChildControl.y;
            }
            if (force || !prev.ChildScale.Equals(next.ChildScale)) {
                group.childScaleWidth = next.ChildScale.x;
                group.childScaleHeight = next.ChildScale.y;
            }
            if (force || !prev.ChildScale.Equals(next.ChildScale)) {
                group.childForceExpandWidth = next.ChildScale.x;
                group.childForceExpandHeight = next.ChildScale.y;
            }
        }
    }
}