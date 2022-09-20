using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        private static readonly Ts _spriteType = (Ts)typeof(Sprite);
        private static readonly Ts _materialType = (Ts)typeof(Material);

        public readonly struct ImageConfig {
            public readonly S SpriteAddress;
            public readonly float4 Color;
            public readonly S MaterialAddress;
            public readonly bool RaycastTarget;
            public readonly float4 RaycastPadding;
            public readonly bool Maskable;
            public readonly ImageType ImageType;
            public readonly bool PreserveAspect;
            public readonly float PixelsPerUnitMultiplier;
            public readonly FillMethod FillMethod;
            public readonly FillOrigin FillOrigin;
            public readonly float FillAmount;
            public readonly bool FillClockwise;
            public readonly bool FillCenter;

            public ImageConfig(
                float4 color,
                S spriteAddress = default,
                S materialAddress = default,
                bool raycastTarget = true,
                float4 raycastPadding = default,
                bool maskable = true,
                ImageType imageType = ImageType.Simple,
                bool preserveAspect = false,
                float pixelsPerUnitMultiplier = 1,
                FillMethod fillMethod = FillMethod.Horizontal,
                FillOrigin fillOrigin = FillOrigin.OriginHorizontal_Left,
                float fillAmount = 0,
                bool fillClockwise = false,
                bool fillCenter = true
            ) {
                SpriteAddress = spriteAddress;
                Color = color;
                MaterialAddress = materialAddress;
                RaycastTarget = raycastTarget;
                RaycastPadding = raycastPadding;
                Maskable = maskable;
                ImageType = imageType;
                PreserveAspect = preserveAspect;
                PixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                FillMethod = fillMethod;
                FillOrigin = fillOrigin;
                FillAmount = fillAmount;
                FillClockwise = fillClockwise;
                FillCenter = fillCenter;
            }
        }

        private static readonly ulong _idImage = CompExtensions.NextId;

        public static Comp Image(ImageConfig config) {
            return new Comp(_idImage, &SyncImage, Mem.C.CopyPtr(config));
        }

        private static void SyncImage(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<Image>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<Image>();
            }

            var image = gameObject.GetComponent<Image>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<ImageConfig>();
            var next = *pNext.Cast<ImageConfig>();

            if (force || !prev.Color.Equals(next.Color)) {
                image.color = Colors.FromFloat4(next.Color);
            }
            if (force || (prev.RaycastTarget != next.RaycastTarget)) {
                image.raycastTarget = next.RaycastTarget;
            }
            if (force || !prev.RaycastPadding.Equals(next.RaycastPadding)) {
                image.raycastPadding = next.RaycastPadding;
            }
            if (force || (prev.Maskable != next.Maskable)) {
                image.maskable = next.Maskable;
            }
            if (force || (prev.ImageType != next.ImageType)) {
                image.type = (Image.Type)next.ImageType;
            }
            if (force || (prev.PreserveAspect != next.PreserveAspect)) {
                image.preserveAspect = next.PreserveAspect;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.PixelsPerUnitMultiplier != next.PixelsPerUnitMultiplier)) {
                image.pixelsPerUnitMultiplier = next.PixelsPerUnitMultiplier;
            }
            if (force || (prev.FillMethod != next.FillMethod)) {
                image.fillMethod = (Image.FillMethod)next.FillMethod;
            }
            if (force || (prev.FillOrigin != next.FillOrigin)) {
                image.fillOrigin = (int)next.FillOrigin;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.FillAmount != next.FillAmount)) {
                image.fillAmount = next.FillAmount;
            }
            if (force || (prev.FillClockwise != next.FillClockwise)) {
                image.fillClockwise = next.FillClockwise;
            }
            if (force || (prev.FillCenter != next.FillCenter)) {
                image.fillCenter = next.FillCenter;
            }

            if (force || (prev.SpriteAddress != next.SpriteAddress)) {
                if (next.SpriteAddress == S.Empty) {
                    image.sprite = null;
                }
                else {
                    AddressablesCache.Load(next.SpriteAddress, _spriteType, gameObject, CLa.New<GameObject, Object>(&HandleImageSpriteLoaded));
                }
            }

            if (force || (prev.MaterialAddress != next.MaterialAddress)) {
                if (next.MaterialAddress == S.Empty) {
                    image.material = null;
                }
                else {
                    AddressablesCache.Load(next.MaterialAddress, _materialType, gameObject, CLa.New<GameObject, Object>(&HandleImageMaterialLoaded));
                }
            }
        }

        private static void HandleImageSpriteLoaded(GameObject gameObject, Object sprite) {
            gameObject.GetComponent<Image>().sprite = (Sprite)sprite;
        }

        private static void HandleImageMaterialLoaded(GameObject gameObject, Object material) {
            gameObject.GetComponent<Image>().material = (Material)material;
        }

        public enum ImageType {
            Simple,
            Sliced,
            Tiled,
            Filled,
        }

        public enum FillMethod {
            Horizontal,
            Vertical,
            Radial90,
            Radial180,
            Radial360,
        }

        public enum FillOrigin {
            // ReSharper disable InconsistentNaming
            OriginHorizontal_Left = 0,
            OriginHorizontal_Right = 1,
            OriginVertical_Bottom = 0,
            OriginVertical_Top = 1,
            Origin90_BottomLeft = 0,
            Origin90_TopLeft = 1,
            Origin90_TopRight = 2,
            Origin90_BottomRight = 3,
            Origin180_Bottom = 0,
            Origin180_Left = 1,
            Origin180_Top = 2,
            Origin180_Right = 3,
            Origin360_Bottom = 0,
            Origin360_Right = 1,
            Origin360_Top = 2,
            Origin360_Left = 3,
            // ReSharper restore InconsistentNaming
        }
    }
}