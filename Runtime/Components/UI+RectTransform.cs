using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct RectTransformConfig {
            public readonly float4 Rect;
            public readonly float2 AnchorMin;
            public readonly float2 AnchorMax;
            public readonly float2 Pivot;
            public readonly quaternion Rotation;
            public readonly float3 Scale;
            public readonly bool Anchored;

            public RectTransformConfig(
                float4 rect,
                float2 anchorMin,
                float2 anchorMax,
                float2 pivot,
                quaternion rotation,
                float3 scale,
                bool anchored
            ) {
                Rect = rect;
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                Pivot = pivot;
                Rotation = rotation;
                Scale = scale;
                Anchored = anchored;
            }
        }

        private static readonly ulong _idRectTransform = CompExtensions.NextId;

        public static Comp RectTransform(RectTransformConfig config) {
            return new Comp(_idRectTransform, &SyncRectTransform, Mem.C.CopyPtr(config));
        }

        public static Comp RectTransform(
            float2 position,
            float2 size,
            float2 pivot,
            quaternion rotation,
            float3 scale
        ) {
            return RectTransform(new RectTransformConfig(
                new float4(position, size), default, default, pivot, rotation, scale, false));
        }

        public static Comp RectTransform(
            float2 position,
            float2 size,
            float2 pivot
        ) {
            return RectTransform(position, size, pivot, quaternion.identity, 1);
        }

        public static Comp RectTransform(
            float2 anchorMin,
            float2 anchorMax,
            float2 offsetMin,
            float2 offsetMax,
            float2 pivot,
            quaternion rotation,
            float3 scale
        ) {
            return RectTransform(new RectTransformConfig(
                new float4(offsetMin, offsetMax), anchorMin, anchorMax, pivot, rotation, scale, true));
        }

        public static Comp RectTransform(
            float2 anchorMin,
            float2 anchorMax,
            float2 offsetMin,
            float2 offsetMax,
            float2 pivot
        ) {
            return RectTransform(anchorMin, anchorMax, offsetMin, offsetMax, pivot, quaternion.identity, 1);
        }

        public static Comp RectTransform(
            float2 anchorMin,
            float2 anchorMax,
            float2 offsetMin,
            float2 offsetMax
        ) {
            return RectTransform(anchorMin, anchorMax, offsetMin, offsetMax, 0.5f);
        }

        public static Comp RectTransform(
            float2 anchorMin,
            float2 anchorMax
        ) {
            return RectTransform(anchorMin, anchorMax, float2.zero, float2.zero, 0.5f);
        }

        private static void SyncRectTransform(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<RectTransform>(gameObject);
                return;
            }
            var rt = gameObject.GetComponent<RectTransform>();

            if ((pPrev == Ptr.Null) && !rt) {
                rt = gameObject.AddComponent<RectTransform>();
            }

            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<RectTransformConfig>();
            var next = *pNext.Cast<RectTransformConfig>();

            if (force || !prev.Rect.Equals(next.Rect)) {
                if (next.Anchored) {
                    rt.offsetMin = next.Rect.xy;
                    rt.offsetMax = next.Rect.zw;
                }
                else {
                    rt.anchoredPosition = next.Rect.xy;
                    rt.sizeDelta = next.Rect.zw;
                }
            }
            if (force || !prev.AnchorMin.Equals(next.AnchorMin)) {
                rt.anchorMin = next.AnchorMin;
            }
            if (force || !prev.AnchorMax.Equals(next.AnchorMax)) {
                rt.anchorMax = next.AnchorMax;
            }
            if (force || !prev.Pivot.Equals(next.Pivot)) {
                rt.pivot = next.Pivot;
            }
            if (force || !prev.Rotation.Equals(next.Rotation)) {
                rt.localRotation = next.Rotation;
            }
            if (force || !prev.Scale.Equals(next.Scale)) {
                rt.localScale = next.Scale;
            }
        }
    }
}