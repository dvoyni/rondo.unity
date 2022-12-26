using Rondo.Unity.Utils;
using UnityEngine;
#if RONDO_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Rondo.Unity.Components {
    public static partial class Rendering {
        public struct Camera : IComp {
            public bool Ortho;
            public float OrthoSizeOrFov;
            public float Near;
            public float Far;
            public bool Main;

            //TODO: rest camera props
            public Camera(
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

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var camera = create 
                        ? gameObject.AddComponent<UnityEngine.Camera>() 
                        : gameObject.GetComponent<UnityEngine.Camera>();
                var prev = create ? default : (Camera)cPrev;

                if (create || (prev.Ortho != Ortho)) {
                    camera.orthographic = Ortho;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.OrthoSizeOrFov != OrthoSizeOrFov)) {
                    if (Ortho) {
                        camera.orthographicSize = OrthoSizeOrFov;
                    }
                    else {
                        camera.fieldOfView = OrthoSizeOrFov;
                    }
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.Near != Near)) {
                    camera.nearClipPlane = Near;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.Far != Far)) {
                    camera.farClipPlane = Far;
                }

                if (create || (prev.Main != Main)) {
                    if (Main) {
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

            public void Remove(IPresenter presenter, GameObject gameObject) {
#if RONDO_URP
                Helpers.DestroySafe<UniversalAdditionalCameraData>(gameObject);
#endif
                Helpers.DestroySafe<UnityEngine.Camera>(gameObject);
            }
        }
    }
}