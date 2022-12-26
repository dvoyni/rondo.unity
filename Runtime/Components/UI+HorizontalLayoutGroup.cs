using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct HorizontalLayoutGroup : IComp {
            public readonly int4 Padding;
            public readonly float Spacing;
            public readonly TextAnchor ChildAlignment;
            public readonly bool ReverseArrangement;
            public readonly bool2 ChildControl;
            public readonly bool2 ChildScale;
            public readonly bool2 ChildExpand;

            public HorizontalLayoutGroup(
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

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var force = cPrev == null;
                var group = force
                        ? gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>()
                        : gameObject.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                var prev = force ? default : (HorizontalLayoutGroup)cPrev;
                if (force || !prev.Padding.Equals(Padding)) {
                    group.padding = new RectOffset(Padding.w, Padding.y, Padding.x, Padding.z);
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (force || (prev.Spacing != Spacing)) {
                    group.spacing = Spacing;
                }
                if (force || (prev.ChildAlignment != ChildAlignment)) {
                    group.childAlignment = (UnityEngine.TextAnchor)ChildAlignment;
                }
                if (force || (prev.ReverseArrangement != ReverseArrangement)) {
                    group.reverseArrangement = ReverseArrangement;
                }
                if (force || !prev.ChildControl.Equals(ChildControl)) {
                    group.childControlWidth = ChildControl.x;
                    group.childControlHeight = ChildControl.y;
                }
                if (force || !prev.ChildScale.Equals(ChildScale)) {
                    group.childScaleWidth = ChildScale.x;
                    group.childScaleHeight = ChildScale.y;
                }
                if (force || !prev.ChildExpand.Equals(ChildExpand)) {
                    group.childForceExpandWidth = ChildExpand.x;
                    group.childForceExpandHeight = ChildExpand.y;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Utils.Helpers.DestroySafe<UnityEngine.UI.HorizontalLayoutGroup>(gameObject);
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