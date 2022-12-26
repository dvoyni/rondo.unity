using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct CanvasScaler : IComp {
            public readonly ScaleMode UIScaleMode;
            public readonly float ScaleFactor;
            public readonly float ReferencePixelsPerUnit;
            public readonly float2 ReferenceResolution;
            public readonly ScreenMatchMode ScreenMatchMode;
            public readonly float MatchWidthOrHeight;
            public readonly Unit PhysicalUnit;
            public readonly float FallbackScreenDPI;
            public readonly float DefaultSpriteDPI;

            public CanvasScaler(
                ScaleMode uiScaleMode = ScaleMode.ScaleWithScreenSize,
                float2 referenceResolution = default,
                float scaleFactor = 1,
                float referencePixelsPerUnit = 100,
                ScreenMatchMode screenMatchMode = ScreenMatchMode.MatchWidthOrHeight,
                float matchWidthOrHeight = 0.5f,
                Unit physicalUnit = Unit.Points,
                float fallbackScreenDPI = 96,
                float defaultSpriteDPI = 96
            ) {
                UIScaleMode = uiScaleMode;
                ScaleFactor = scaleFactor;
                ReferencePixelsPerUnit = referencePixelsPerUnit;
                ReferenceResolution = referenceResolution;
                ScreenMatchMode = screenMatchMode;
                MatchWidthOrHeight = matchWidthOrHeight;
                PhysicalUnit = physicalUnit;
                FallbackScreenDPI = fallbackScreenDPI;
                DefaultSpriteDPI = defaultSpriteDPI;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var scaler = create ? gameObject.AddComponent<UnityEngine.UI.CanvasScaler>() : gameObject.GetComponent<UnityEngine.UI.CanvasScaler>();
                var prev = create ? default : (CanvasScaler)cPrev;

                if (create || (prev.UIScaleMode != UIScaleMode)) {
                    scaler.uiScaleMode = (UnityEngine.UI.CanvasScaler.ScaleMode)UIScaleMode;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.ScaleFactor != ScaleFactor)) {
                    scaler.scaleFactor = ScaleFactor;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.ReferencePixelsPerUnit != ReferencePixelsPerUnit)) {
                    scaler.referencePixelsPerUnit = ReferencePixelsPerUnit;
                }
                if (create || !prev.ReferenceResolution.Equals(ReferenceResolution)) {
                    scaler.referenceResolution = ReferenceResolution;
                }
                if (create || (prev.ScreenMatchMode != ScreenMatchMode)) {
                    scaler.screenMatchMode = (UnityEngine.UI.CanvasScaler.ScreenMatchMode)ScreenMatchMode;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.MatchWidthOrHeight != MatchWidthOrHeight)) {
                    scaler.matchWidthOrHeight = MatchWidthOrHeight;
                }
                if (create || (prev.PhysicalUnit != PhysicalUnit)) {
                    scaler.physicalUnit = (UnityEngine.UI.CanvasScaler.Unit)PhysicalUnit;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.FallbackScreenDPI != FallbackScreenDPI)) {
                    scaler.fallbackScreenDPI = FallbackScreenDPI;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.DefaultSpriteDPI != DefaultSpriteDPI)) {
                    scaler.defaultSpriteDPI = DefaultSpriteDPI;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.CanvasScaler>(gameObject);
            }
        }

        public enum ScaleMode {
            ConstantPixelSize,
            ScaleWithScreenSize,
            ConstantPhysicalSize,
        }

        public enum ScreenMatchMode {
            MatchWidthOrHeight = 0,
            Expand = 1,
            Shrink = 2
        }

        public enum Unit {
            Centimeters,
            Millimeters,
            Inches,
            Points,
            Picas
        }
    }
}