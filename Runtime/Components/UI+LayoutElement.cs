using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct LayoutElement : IComp {
            public readonly bool IgnoreLayout;
            public readonly int LayoutPriority;
            public readonly float2 Min;
            public readonly float2 Flexible;
            public readonly float2 Preferred;

            public LayoutElement(
                float2 preferred = default,
                bool ignoreLayout = false,
                int layoutPriority = 0,
                float2 min = default,
                float2 flexible = default
            ) {
                IgnoreLayout = ignoreLayout;
                LayoutPriority = layoutPriority;
                Min = min;
                Flexible = flexible;
                Preferred = preferred;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var force = cPrev == null;
                var ltel = force ? gameObject.AddComponent<UnityEngine.UI.LayoutElement>() : gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
                var prev = force ? default : (LayoutElement)cPrev;

                if (force || (prev.IgnoreLayout != IgnoreLayout)) {
                    ltel.ignoreLayout = IgnoreLayout;
                }
                if (force || (prev.LayoutPriority != LayoutPriority)) {
                    ltel.layoutPriority = LayoutPriority;
                }
                if (force || !prev.Min.Equals(Min)) {
                    ltel.minWidth = Min.x;
                    ltel.minHeight = Min.y;
                }
                if (force || !prev.Flexible.Equals(Flexible)) {
                    ltel.flexibleWidth = prev.Flexible.x;
                    ltel.flexibleHeight = prev.Flexible.y;
                }
                if (force || !prev.Preferred.Equals(Preferred)) {
                    ltel.preferredWidth = Preferred.x;
                    ltel.preferredHeight = Preferred.y;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.LayoutElement>(gameObject);
            }
        }
    }
}