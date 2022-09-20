using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    //TODO: rest colliders
    //TODO: disable physics
    public static unsafe partial class Rendering {
        public readonly struct TransformConfig {
            public readonly float3 Position;
            public readonly quaternion Rotation;
            public readonly float3 Scale;

            public TransformConfig(
                float3 position,
                quaternion rotation,
                float3 scale
            ) {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }
        }

        private static readonly ulong _idTransform = CompExtensions.NextId;

        public static Comp Transform(TransformConfig config) {
            return new Comp(_idTransform, &SyncTransform, Mem.C.CopyPtr(config));
        }

        public static Comp Transform(float3 position) {
            return Transform(position, quaternion.identity);
        }

        public static Comp Transform(quaternion rotation) {
            return Transform(float3.zero, rotation);
        }

        public static Comp Transform(float3 position, quaternion rotation) {
            return Transform(position, rotation, 1);
        }

        public static Comp Transform(float3 position, float scale) {
            return Transform(position, new float3(scale, scale, scale));
        }

        public static Comp Transform(float3 position, float3 scale) {
            return Transform(position, quaternion.identity, scale);
        }

        public static Comp Transform(float3 position, float zRotationRad, float scale) {
            return Transform(position, quaternion.AxisAngle(new float3(0, 0, 1), zRotationRad), scale);
        }

        public static Comp Transform(float3 position, quaternion rotation, float scale) {
            return Transform(position, rotation, new float3(scale, scale, scale));
        }

        public static Comp Transform(float3 position, quaternion rotation, float3 scale) {
            return Transform(new TransformConfig(position, rotation, scale));
        }

        private static void SyncTransform(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }

            if (pNext == Ptr.Null) {
                return;
            }

            var tf = gameObject.transform;
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<TransformConfig>();
            var next = *pNext.Cast<TransformConfig>();

            if (force || !prev.Position.Equals(next.Position)) {
                tf.localPosition = next.Position;
            }
            if (force || !prev.Rotation.Equals(next.Rotation)) {
                tf.localRotation = next.Rotation;
            }
            if (force || !prev.Scale.Equals(next.Scale)) {
                tf.localScale = next.Scale;
            }
        }
    }
}