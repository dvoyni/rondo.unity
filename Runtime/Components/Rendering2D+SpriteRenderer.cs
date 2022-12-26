using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static class Rendering2D {
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

        public readonly struct SpriteRenderer : IComp {
            public readonly string SpriteAddress;
            public readonly float4 Color;
            public readonly bool2 Flip;
            public readonly string MaterialAddress;
            public readonly SpriteDrawMode DrawMode;
            public readonly int SortingLayer;
            public readonly int OrderInLayer;
            public readonly SpriteMaskInteraction MaskInteraction;
            public readonly SpriteSortPoint SortPoint;
            public readonly float2 Size;

            public SpriteRenderer(
                string spriteAddress,
                bool2 flip = default,
                string materialAddress = default,
                SpriteDrawMode drawMode = SpriteDrawMode.Simple,
                int sortingLayer = 0,
                int orderInLayer = 0,
                SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None,
                SpriteSortPoint sortPoint = SpriteSortPoint.Center,
                float2 size = default
            ) : this(
                spriteAddress, 1, flip, materialAddress, drawMode, sortingLayer, orderInLayer,
                maskInteraction, sortPoint, size
            ) { }

            public SpriteRenderer(
                string spriteAddress,
                float4 color,
                bool2 flip = default,
                string materialAddress = default,
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

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var spriteRenderer = create
                        ? gameObject.AddComponent<UnityEngine.SpriteRenderer>()
                        : gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                var prev = create ? default : (SpriteRenderer)cPrev;

                if (create || !prev.Color.Equals(Color)) {
                    spriteRenderer.color = Colors.FromFloat4(Color);
                }
                if (create || !prev.Flip.Equals(Flip)) {
                    spriteRenderer.flipX = Flip.x;
                    spriteRenderer.flipY = Flip.y;
                }
                if (create || (prev.DrawMode != DrawMode)) {
                    spriteRenderer.drawMode = (UnityEngine.SpriteDrawMode)DrawMode;
                }
                if (create || (prev.SortingLayer != SortingLayer)) {
                    spriteRenderer.sortingLayerID = SortingLayer;
                }
                if (create || (prev.OrderInLayer != OrderInLayer)) {
                    spriteRenderer.sortingOrder = OrderInLayer;
                }
                if (create || (prev.MaskInteraction != MaskInteraction)) {
                    spriteRenderer.maskInteraction = (UnityEngine.SpriteMaskInteraction)MaskInteraction;
                }
                if (create || (prev.SortPoint != SortPoint)) {
                    spriteRenderer.spriteSortPoint = (UnityEngine.SpriteSortPoint)SortPoint;
                }
                if (create || !prev.Size.Equals(Size)) {
                    spriteRenderer.size = Size;
                }

                if (create || (prev.SpriteAddress != SpriteAddress)) {
                    if (string.IsNullOrEmpty(SpriteAddress)) {
                        spriteRenderer.sprite = null;
                    }
                    else {
                        var size = Size;
                        AddressablesCache.Load<Sprite>(SpriteAddress, sprite => {
                            spriteRenderer.sprite = (Sprite)sprite;
                            spriteRenderer.size = size;
                        });
                    }
                }

                if (create || (prev.MaterialAddress != MaterialAddress)) {
                    if (string.IsNullOrEmpty(MaterialAddress)) {
                        spriteRenderer.material = _defaultSpriteMaterial;
                    }
                    else {
                        AddressablesCache.Load<Material>(
                            MaterialAddress, material => spriteRenderer.material = (Material)material
                        );
                    }
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.SpriteRenderer>(gameObject);
            }
        }
    }
}