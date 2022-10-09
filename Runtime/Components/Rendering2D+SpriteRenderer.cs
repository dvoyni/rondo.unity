using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static unsafe class Rendering2D {
        private static readonly Ts _spriteType = Ts.OfUnmanaged(typeof(Sprite));
        private static readonly Ts _materialType = Ts.OfUnmanaged(typeof(Material));
        private static Material _defaultSpriteMaterial = new(Shader.Find("Sprites/Default"));

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            _defaultSpriteMaterial = new(Shader.Find("Sprites/Default"));
        }
#endif

        public enum SpriteDrawMode {
            Simple,
            Sliced,
            Tiled,
        }

        public enum SpriteMaskInteraction {
            None,
            VisibleInsideMask,
            VisibleOutsideMask,
        }

        public enum SpriteSortPoint {
            Center,
            Pivot,
        }

        public readonly struct SpriteRendererConfig {
            public readonly S SpriteAddress;
            public readonly float4 Color;
            public readonly bool2 Flip;
            public readonly S MaterialAddress;
            public readonly SpriteDrawMode DrawMode;
            public readonly int SortingLayer;
            public readonly int OrderInLayer;
            public readonly SpriteMaskInteraction MaskInteraction;
            public readonly SpriteSortPoint SortPoint;
            public readonly float2 Size;

            public SpriteRendererConfig(
                float4 color,
                S spriteAddress = default,
                bool2 flip = default,
                S materialAddress = default,
                SpriteDrawMode drawMode = SpriteDrawMode.Simple,
                int sortingLayer = 0,
                int orderInLayer = 0,
                SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None,
                SpriteSortPoint sortPoint = SpriteSortPoint.Center,
                float2 size = default
            ) {
                SpriteAddress = spriteAddress;
                Color = color;
                Flip = flip;
                MaterialAddress = materialAddress;
                DrawMode = drawMode;
                SortingLayer = sortingLayer;
                OrderInLayer = orderInLayer;
                MaskInteraction = maskInteraction;
                SortPoint = sortPoint;
                Size = size;
            }
        }

        private static readonly ulong _id = CompExtensions.NextId;

        public static Comp SpriteRenderer(SpriteRendererConfig config) {
            return new Comp(_id, &SyncSpriteRenderer, Mem.C.CopyPtr(config));
        }

        private static void SyncSpriteRenderer(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<SpriteRenderer>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<SpriteRenderer>();
            }
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<SpriteRendererConfig>();
            var next = *pNext.Cast<SpriteRendererConfig>();
            var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            if (force || !prev.Color.Equals(next.Color)) {
                spriteRenderer.color = Colors.FromFloat4(next.Color);
            }
            if (force || !prev.Flip.Equals(next.Flip)) {
                spriteRenderer.flipX = next.Flip.x;
                spriteRenderer.flipY = next.Flip.y;
            }
            if (force || (prev.DrawMode != next.DrawMode)) {
                spriteRenderer.drawMode = (UnityEngine.SpriteDrawMode)next.DrawMode;
            }
            if (force || (prev.SortingLayer != next.SortingLayer)) {
                spriteRenderer.sortingLayerID = next.SortingLayer;
            }
            if (force || (prev.OrderInLayer != next.OrderInLayer)) {
                spriteRenderer.sortingOrder = next.OrderInLayer;
            }
            if (force || (prev.MaskInteraction != next.MaskInteraction)) {
                spriteRenderer.maskInteraction = (UnityEngine.SpriteMaskInteraction)next.MaskInteraction;
            }
            if (force || (prev.SortPoint != next.SortPoint)) {
                spriteRenderer.spriteSortPoint = (UnityEngine.SpriteSortPoint)next.SortPoint;
            }
            if (force || !prev.Size.Equals(next.Size)) {
                spriteRenderer.size = next.Size;
            }

            if (force || (prev.SpriteAddress != next.SpriteAddress)) {
                if (next.SpriteAddress == S.Empty) {
                    spriteRenderer.sprite = null;
                }
                else {
                    AddressablesCache.Load(next.SpriteAddress, _spriteType, gameObject, HandleSpriteRendererSpriteLoaded(next.Size));
                }
            }

            if (force || (prev.MaterialAddress != next.MaterialAddress)) {
                if (next.MaterialAddress == S.Empty) {
                    spriteRenderer.material = _defaultSpriteMaterial;
                }
                else {
                    AddressablesCache.Load(
                        next.MaterialAddress, _materialType, gameObject, CLa.New<GameObject, Object>(&HandleSpriteRendererMaterialLoaded)
                    );
                }
            }
        }

        private static CLa<GameObject, Object> HandleSpriteRendererSpriteLoaded(float2 size) {
            static void Impl(GameObject gameObject, Object sprite, float2* size) {
                var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = (Sprite)sprite;
                spriteRenderer.size = *size;
            }

            return CLa.New<GameObject, Object, float2>(&Impl, size);
        }

        private static void HandleSpriteRendererMaterialLoaded(GameObject gameObject, Object material) {
            gameObject.GetComponent<SpriteRenderer>().material = (Material)material;
        }
    }
}