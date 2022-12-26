using Rondo.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct Canvas : IComp {
            public readonly RenderMode RenderMode;
            public readonly bool PixelPerfect;
            public readonly int TargetDisplay;
            public readonly ObjRef CameraRef;
            public readonly int SortingLayer;
            public readonly int OrderInLayer;

            public Canvas(
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

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var force = cPrev == null;

                var canvas = force ? gameObject.AddComponent<UnityEngine.Canvas>() : gameObject.GetComponent<UnityEngine.Canvas>();
                var prev = force ? default : (Canvas)cPrev;

                if (
                    force
                    || (prev.RenderMode != RenderMode)
                    || ((UnityEngine.RenderMode)RenderMode != canvas.renderMode)
                ) {
                    canvas.renderMode = (UnityEngine.RenderMode)RenderMode;
                }

                if (force || (prev.PixelPerfect != PixelPerfect)) {
                    canvas.pixelPerfect = PixelPerfect;
                }

                if (force || (prev.TargetDisplay != TargetDisplay)) {
                    canvas.targetDisplay = TargetDisplay;
                }

                if (force || (prev.SortingLayer != SortingLayer)) {
                    canvas.sortingLayerID = SortingLayer;
                }

                if (force || (prev.OrderInLayer != OrderInLayer)) {
                    canvas.sortingOrder = OrderInLayer;
                }

                if (CameraRef == ObjRef.NoRef) {
                    if (force || (prev.CameraRef != ObjRef.NoRef)) {
                        canvas.worldCamera = null;
                    }
                }
                else {
                    presenter.RequestObjRef(CameraRef, cameraObj => {
                        // ReSharper disable once Unity.NoNullPropagation
                        var cam = cameraObj?.GetComponent<Camera>();
                        gameObject.GetComponent<UnityEngine.Canvas>().worldCamera = cam;
                    });
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.CanvasScaler>(gameObject);
                Helpers.DestroySafe<GraphicRaycaster>(gameObject);
                Helpers.DestroySafe<UnityEngine.Canvas>(gameObject);
            }
        }

        public enum RenderMode {
            ScreenSpaceOverlay,
            ScreenSpaceCamera,
            WorldSpace,
        }
    }
}