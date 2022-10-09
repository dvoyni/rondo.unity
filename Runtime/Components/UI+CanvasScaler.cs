using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct CanvasScalerConfig {
            public readonly ScaleMode UIScaleMode;
            public readonly float ScaleFactor;
            public readonly float ReferencePixelsPerUnit;
            public readonly float2 ReferenceResolution;
            public readonly ScreenMatchMode ScreenMatchMode;
            public readonly float MatchWidthOrHeight;
            public readonly Unit PhysicalUnit;
            public readonly float FallbackScreenDPI;
            public readonly float DefaultSpriteDPI;

            public CanvasScalerConfig(
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
        }

        private static readonly ulong _idCanvasScaler = CompExtensions.NextId;

        public static Comp CanvasScaler(CanvasScalerConfig config) {
            return new Comp(_idCanvasScaler, &SyncCanvasScaler, Mem.C.CopyPtr(config));
        }

        private static void SyncCanvasScaler(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<CanvasScaler>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<CanvasScaler>();
            }

            var scaler = gameObject.GetComponent<CanvasScaler>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<CanvasScalerConfig>();
            var next = *pNext.Cast<CanvasScalerConfig>();

            if (force || (prev.UIScaleMode != next.UIScaleMode)) {
                scaler.uiScaleMode = (CanvasScaler.ScaleMode)next.UIScaleMode;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.ScaleFactor != next.ScaleFactor)) {
                scaler.scaleFactor = next.ScaleFactor;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.ReferencePixelsPerUnit != next.ReferencePixelsPerUnit)) {
                scaler.referencePixelsPerUnit = next.ReferencePixelsPerUnit;
            }
            if (force || !prev.ReferenceResolution.Equals(next.ReferenceResolution)) {
                scaler.referenceResolution = next.ReferenceResolution;
            }
            if (force || (prev.ScreenMatchMode != next.ScreenMatchMode)) {
                scaler.screenMatchMode = (CanvasScaler.ScreenMatchMode)next.ScreenMatchMode;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.MatchWidthOrHeight != next.MatchWidthOrHeight)) {
                scaler.matchWidthOrHeight = next.MatchWidthOrHeight;
            }
            if (force || (prev.PhysicalUnit != next.PhysicalUnit)) {
                scaler.physicalUnit = (CanvasScaler.Unit)next.PhysicalUnit;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.FallbackScreenDPI != next.FallbackScreenDPI)) {
                scaler.fallbackScreenDPI = next.FallbackScreenDPI;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.DefaultSpriteDPI != next.DefaultSpriteDPI)) {
                scaler.defaultSpriteDPI = next.DefaultSpriteDPI;
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