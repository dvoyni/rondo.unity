using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Debug = Rondo.Unity.Utils.Debug;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct Image : IComp {
            public readonly string SpriteAddress;
            public readonly float4 Color;
            public readonly string MaterialAddress;
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

            public Image(
                string spriteAddress,
                float4 color,
                string materialAddress = default,
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

            public Image(
                string spriteAddress,
                string materialAddress = default,
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
            ) : this(
                spriteAddress, 1, materialAddress, raycastTarget, raycastPadding, maskable, imageType, preserveAspect,
                pixelsPerUnitMultiplier, fillMethod, fillOrigin, fillAmount, fillClockwise, fillCenter
            ) { }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var image = create
                        ? gameObject.AddComponent<UnityEngine.UI.Image>()
                        : gameObject.GetComponent<UnityEngine.UI.Image>();
                if (!image) {
                    Debug.Log("WTF");
                }

                var prev = create ? default : (Image)cPrev;

                if (create || !prev.Color.Equals(Color)) {
                    image.color = Colors.FromFloat4(Color);
                }
                if (create || (prev.RaycastTarget != RaycastTarget)) {
                    image.raycastTarget = RaycastTarget;
                }
                if (create || !prev.RaycastPadding.Equals(RaycastPadding)) {
                    image.raycastPadding = RaycastPadding;
                }
                if (create || (prev.Maskable != Maskable)) {
                    image.maskable = Maskable;
                }
                if (create || (prev.ImageType != ImageType)) {
                    image.type = (UnityEngine.UI.Image.Type)ImageType;
                }
                if (create || (prev.PreserveAspect != PreserveAspect)) {
                    image.preserveAspect = PreserveAspect;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.PixelsPerUnitMultiplier != PixelsPerUnitMultiplier)) {
                    image.pixelsPerUnitMultiplier = PixelsPerUnitMultiplier;
                }
                if (create || (prev.FillMethod != FillMethod)) {
                    image.fillMethod = (UnityEngine.UI.Image.FillMethod)FillMethod;
                }
                if (create || (prev.FillOrigin != FillOrigin)) {
                    image.fillOrigin = (int)FillOrigin;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.FillAmount != FillAmount)) {
                    image.fillAmount = FillAmount;
                }
                if (create || (prev.FillClockwise != FillClockwise)) {
                    image.fillClockwise = FillClockwise;
                }
                if (create || (prev.FillCenter != FillCenter)) {
                    image.fillCenter = FillCenter;
                }

                if (create || (prev.SpriteAddress != SpriteAddress)) {
                    if (string.IsNullOrEmpty(SpriteAddress)) {
                        image.sprite = null;
                    }
                    else {
                        AddressablesCache.Load<Sprite>(SpriteAddress, sprite => image.sprite = (Sprite)sprite);
                    }
                }

                if (create || (prev.MaterialAddress != MaterialAddress)) {
                    if (string.IsNullOrEmpty(MaterialAddress)) {
                        image.material = null;
                    }
                    else {
                        AddressablesCache.Load<Material>(
                            MaterialAddress,
                            material => image.material = (Material)material
                        );
                    }
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.Image>(gameObject);
            }
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