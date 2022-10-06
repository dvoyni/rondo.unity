using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Subs {
    public static unsafe class Display {
        private static int3 _resolution;
        private static float4x4 _worldToCameraMatrix;
        private static float4x4 _projectionMatrix;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            _resolution = 0;
            _worldToCameraMatrix = 0;
            _projectionMatrix = 0;
        }
#endif

        static Display() {
            Lifecycle.OnUpdate += CheckResolution;
            Lifecycle.OnUpdate += CheckCamera;
        }

        public static Sub ResolutionChanged<TMsg>(delegate*<ResolutionChangedData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }

        private static void CheckResolution(float deltaTime, IPresenter presenter) {
            var resolution = new int3(Screen.width, Screen.height, Screen.currentResolution.refreshRate);
            if (!_resolution.Equals(resolution)) {
                _resolution = resolution;
                presenter.Messenger.TriggerSub(new ResolutionChangedData(resolution.xy, resolution.z));
            }
        }

        public readonly struct ResolutionChangedData {
            public readonly int2 Size;
            public readonly int RefreshRate;

            public ResolutionChangedData(int2 size, int refreshRate) {
                Size = size;
                RefreshRate = refreshRate;
            }
        }

        public static Sub CameraChanged<TMsg>(delegate*<CameraChangedData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }

        private static void CheckCamera(float deltaTime, IPresenter presenter) {
            var cam = presenter.Camera;
            if (cam) {
                var projection = (float4x4)cam.projectionMatrix;
                var view = (float4x4)cam.worldToCameraMatrix;

                if (!_projectionMatrix.Equals(projection) || !_worldToCameraMatrix.Equals(view)) {
                    _projectionMatrix = projection;
                    _worldToCameraMatrix = view;
                    presenter.Messenger.TriggerSub(new CameraChangedData(new CameraMx(_worldToCameraMatrix, _projectionMatrix)));
                }
            }
        }

        public readonly struct CameraMx {
            public readonly float4x4 WorldToCamera;
            public readonly float4x4 Projection;

            public CameraMx(float4x4 worldToCamera, float4x4 projection) {
                WorldToCamera = worldToCamera;
                Projection = projection;
            }
        }

        public readonly struct CameraChangedData {
            public readonly CameraMx Camera;

            public CameraChangedData(CameraMx camera) {
                Camera = camera;
            }
        }
    }
}