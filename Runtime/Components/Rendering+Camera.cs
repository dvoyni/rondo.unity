using Rondo.Core.Memory;
using UnityEngine;
#if RONDO_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Rondo.Unity.Components {
    public static unsafe partial class Rendering {
        public struct CameraConfig {
            public bool Ortho;
            public float OrthoSizeOrFov;
            public float Near;
            public float Far;
            public bool Main;

            //TODO: rest camera props
            public CameraConfig(
                bool ortho,
                float orthoSizeOrFov = 60,
                float near = 0.3f,
                float far = 1000,
                bool main = true
            ) {
                Ortho = ortho;
                OrthoSizeOrFov = orthoSizeOrFov;
                Near = near;
                Far = far;
                Main = main;
            }
        }

        private static readonly ulong _idCamera = CompExtensions.NextId;

        public static Comp Camera(CameraConfig config) {
            return new Comp(_idCamera, &SyncCamera, Mem.C.CopyPtr(config));
        }

        public static Comp OrthoCamera(float orthoSize, float near, float far) {
            return Camera(new CameraConfig(orthoSizeOrFov: orthoSize, near: near, far: far, ortho: true));
        }

        public static Comp PerspectiveCamera(float fov, float near, float far) {
            return Camera(new CameraConfig(orthoSizeOrFov: fov, near: near, far: far, ortho: false));
        }

        private static void SyncCamera(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }

            if (pNext == Ptr.Null) {
#if RONDO_URP
                Utils.Utils.DestroySafe<UniversalAdditionalCameraData>(gameObject);
#endif
                Utils.Utils.DestroySafe<Camera>(gameObject);
                return;
            }

            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<Camera>();
            }

            var camera = gameObject.GetComponent<Camera>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<CameraConfig>();
            var next = *pNext.Cast<CameraConfig>();

            if (force || (prev.Ortho != next.Ortho)) {
                camera.orthographic = next.Ortho;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.OrthoSizeOrFov != next.OrthoSizeOrFov)) {
                if (next.Ortho) {
                    camera.orthographicSize = next.OrthoSizeOrFov;
                }
                else {
                    camera.fieldOfView = next.OrthoSizeOrFov;
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.Near != next.Near)) {
                camera.nearClipPlane = next.Near;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.Far != next.Far)) {
                camera.farClipPlane = next.Far;
            }

            if (force || (prev.Main != next.Main)) {
                if (next.Main) {
                    gameObject.tag = "MainCamera";
                    presenter.Camera = camera;
                }
                else {
                    gameObject.tag = string.Empty;
                    if (presenter.Camera == camera) {
                        presenter.Camera = null;
                    }
                }
            }
        }
    }
}