using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct RectTransform : IComp {
            public readonly float4 Rect;
            public readonly float2 AnchorMin;
            public readonly float2 AnchorMax;
            public readonly float2 Pivot;
            public readonly quaternion Rotation;
            public readonly float3 Scale;
            public readonly bool Anchored;

            public RectTransform(
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

            public RectTransform(
                float2 position,
                float2 size,
                float2 pivot,
                quaternion rotation,
                float3 scale
            ) : this(new float4(position, size), default, default, pivot, rotation, scale, false) { }

            public RectTransform(
                float2 position,
                float2 size,
                float2 pivot
            ) : this(position, size, pivot, quaternion.identity, 1) { }

            public RectTransform(
                float2 anchorMin,
                float2 anchorMax,
                float2 offsetMin,
                float2 offsetMax,
                float2 pivot,
                quaternion rotation,
                float3 scale
            ) : this(new float4(offsetMin, offsetMax), anchorMin, anchorMax, pivot, rotation, scale, true) { }

            public RectTransform(
                float2 anchorMin,
                float2 anchorMax,
                float2 offsetMin,
                float2 offsetMax,
                float2 pivot
            ) : this(anchorMin, anchorMax, offsetMin, offsetMax, pivot, quaternion.identity, 1) { }

            public RectTransform(
                float2 anchorMin,
                float2 anchorMax,
                float2 offsetMin,
                float2 offsetMax
            ) : this(anchorMin, anchorMax, offsetMin, offsetMax, 0.5f) { }

            public RectTransform(
                float2 anchorMin,
                float2 anchorMax
            ) : this(anchorMin, anchorMax, float2.zero, float2.zero, 0.5f) { }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var rt = gameObject.GetComponent<UnityEngine.RectTransform>();
                if (create && !rt) {
                    rt = gameObject.AddComponent<UnityEngine.RectTransform>();
                }
                var prev = create ? default : (RectTransform)cPrev;

                if (create || !prev.Rect.Equals(Rect)) {
                    if (Anchored) {
                        rt.offsetMin = Rect.xy;
                        rt.offsetMax = Rect.zw;
                    }
                    else {
                        rt.anchoredPosition = Rect.xy;
                        rt.sizeDelta = Rect.zw;
                    }
                }
                if (create || !prev.AnchorMin.Equals(AnchorMin)) {
                    rt.anchorMin = AnchorMin;
                }
                if (create || !prev.AnchorMax.Equals(AnchorMax)) {
                    rt.anchorMax = AnchorMax;
                }
                if (create || !prev.Pivot.Equals(Pivot)) {
                    rt.pivot = Pivot;
                }
                if (create || !prev.Rotation.Equals(Rotation)) {
                    rt.localRotation = Rotation;
                }
                if (create || !prev.Scale.Equals(Scale)) {
                    rt.localScale = Scale;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Utils.Helpers.DestroySafe<UnityEngine.RectTransform>(gameObject);
            }
        }
    }
}