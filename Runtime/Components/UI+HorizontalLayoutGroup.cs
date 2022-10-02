using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct LayoutGroupConfig {
            public readonly int4 Padding;
            public readonly float Spacing;
            public readonly TextAnchor ChildAlignment;
            public readonly bool ReverseArrangement;
            public readonly bool2 ChildControl;
            public readonly bool2 ChildScale;
            public readonly bool2 ChildExpand;

            public LayoutGroupConfig(
                TextAnchor childAlignment = default,
                int4 padding = default,
                float spacing = 0,
                bool reverseArrangement = false,
                bool2 childControl = default,
                bool2 childScale = default,
                bool2 childExpand = default
            ) {
                Padding = padding;
                Spacing = spacing;
                ChildAlignment = childAlignment;
                ReverseArrangement = reverseArrangement;
                ChildControl = childControl;
                ChildScale = childScale;
                ChildExpand = childExpand;
            }
        }

        private static readonly ulong _idHorizontalLayoutGroup = CompExtensions.NextId;

        public static Comp HorizontalLayoutGroup(LayoutGroupConfig config) {
            return new Comp(_idHorizontalLayoutGroup, &SyncHorizontalLayoutGroup, Mem.C.CopyPtr(config));
        }

        private static void SyncHorizontalLayoutGroup(
            IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext
        ) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<HorizontalLayoutGroup>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            var group = gameObject.GetComponent<HorizontalLayoutGroup>();
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
            if (force || !prev.ChildExpand.Equals(next.ChildExpand)) {
                group.childForceExpandWidth = next.ChildExpand.x;
                group.childForceExpandHeight = next.ChildExpand.y;
            }
        }

        public enum TextAnchor {
            UpperLeft,
            UpperCenter,
            UpperRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            LowerLeft,
            LowerCenter,
            LowerRight,
        }
    }
}