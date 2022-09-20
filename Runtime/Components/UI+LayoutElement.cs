using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct LayoutElementConfig {
            public readonly bool IgnoreLayout;
            public readonly int LayoutPriority;
            public readonly float2 Min;
            public readonly float2 Flexible;
            public readonly float2 Preferred;

            public LayoutElementConfig(
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
        }

        private static readonly ulong _idLayoutElement = CompExtensions.NextId;

        public static Comp LayoutElement(LayoutElementConfig config) {
            return new Comp(_idLayoutElement, &SyncLayoutElement, Mem.C.CopyPtr(config));
        }

        private static void SyncLayoutElement(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<LayoutElement>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<LayoutElement>();
            }

            var ltel = gameObject.GetComponent<LayoutElement>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<LayoutElementConfig>();
            var next = *pNext.Cast<LayoutElementConfig>();

            if (force || (prev.IgnoreLayout != next.IgnoreLayout)) {
                ltel.ignoreLayout = next.IgnoreLayout;
            }
            if (force || (prev.LayoutPriority != next.LayoutPriority)) {
                ltel.layoutPriority = next.LayoutPriority;
            }
            if (force || !prev.Min.Equals(next.Min)) {
                ltel.minWidth = next.Min.x;
                ltel.minHeight = next.Min.y;
            }
            if (force || !prev.Flexible.Equals(next.Flexible)) {
                ltel.flexibleWidth = prev.Flexible.x;
                ltel.flexibleHeight = prev.Flexible.y;
            }
            if (force || !prev.Preferred.Equals(next.Preferred)) {
                ltel.preferredWidth = next.Preferred.x;
                ltel.preferredHeight = next.Preferred.y;
            }
        }
    }
}