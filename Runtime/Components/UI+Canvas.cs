using Rondo.Core.Lib;
using Rondo.Core.Memory;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public enum RenderMode {
            ScreenSpaceOverlay,
            ScreenSpaceCamera,
            WorldSpace,
        }

        public readonly struct CanvasConfig {
            public readonly RenderMode RenderMode;
            public readonly bool PixelPerfect;
            public readonly int TargetDisplay;
            public readonly ObjRef CameraRef;
            public readonly int SortingLayer;
            public readonly int OrderInLayer;

            public CanvasConfig(
                RenderMode renderMode,
                bool pixelPerfect = false,
                int targetDisplay = 0,
                ObjRef cameraRef = default,
                int sortingLayer = 0,
                int orderInLayer = 0
            ) {
                RenderMode = renderMode;
                PixelPerfect = pixelPerfect;
                TargetDisplay = targetDisplay;
                CameraRef = cameraRef;
                SortingLayer = sortingLayer;
                OrderInLayer = orderInLayer;
            }
        }

        private static readonly ulong _idCanvas = CompExtensions.NextId;

        public static Comp Canvas(CanvasConfig config) {
            return new Comp(_idCanvas, &SyncCanvas, Mem.C.CopyPtr(config));
        }

        private static void SyncCanvas(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<CanvasScaler>(gameObject);
                Utils.Utils.DestroySafe<GraphicRaycaster>(gameObject);
                Utils.Utils.DestroySafe<Canvas>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<Canvas>();
            }

            var canvas = gameObject.GetComponent<Canvas>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<CanvasConfig>();
            var next = *pNext.Cast<CanvasConfig>();

            if (
                force
                || (prev.RenderMode != next.RenderMode)
                || ((UnityEngine.RenderMode)next.RenderMode != canvas.renderMode)
            ) {
                canvas.renderMode = (UnityEngine.RenderMode)next.RenderMode;
            }

            if (force || (prev.PixelPerfect != next.PixelPerfect)) {
                canvas.pixelPerfect = next.PixelPerfect;
            }

            if (force || (prev.TargetDisplay != next.TargetDisplay)) {
                canvas.targetDisplay = next.TargetDisplay;
            }

            if (force || (prev.SortingLayer != next.SortingLayer)) {
                canvas.sortingLayerID = next.SortingLayer;
            }

            if (force || (prev.OrderInLayer != next.OrderInLayer)) {
                canvas.sortingOrder = next.OrderInLayer;
            }

            if (next.CameraRef == ObjRef.NoRef) {
                if (force || (prev.CameraRef != ObjRef.NoRef)) {
                    canvas.worldCamera = null;
                }
            }
            else {
                presenter.RequestObjRef(next.CameraRef, gameObject, Ca.New<GameObject, GameObject>(&HandleCameraRef));
            }
        }

        private static void HandleCameraRef(GameObject gameObject, GameObject camera) {
            var cam = ReferenceEquals(camera, null) ? null : camera.GetComponent<Camera>();
            gameObject.GetComponent<Canvas>().worldCamera = cam;
        }
    }
}