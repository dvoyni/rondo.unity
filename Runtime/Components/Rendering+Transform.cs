using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static partial class Rendering {
        public readonly struct Transform : IComp {
            public readonly float3 Position;
            public readonly quaternion Rotation;
            public readonly float3 Scale;

            public Transform(
                float3 position,
                quaternion rotation,
                float3 scale
            ) {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }

            public Transform(float3 position)
                    : this(position, quaternion.identity) { }

            public Transform(quaternion rotation)
                    : this(float3.zero, rotation) { }

            public Transform(float3 position, quaternion rotation)
                    : this(position, rotation, 1) { }

            public Transform(float3 position, float scale)
                    : this(position, new float3(scale, scale, scale)) { }

            public Transform(float3 position, float3 scale)
                    : this(position, quaternion.identity, scale) { }

            public Transform(float3 position, float zRotationRad, float scale)
                    : this(position, quaternion.AxisAngle(new float3(0, 0, 1), zRotationRad), scale) { }

            public Transform(float3 position, quaternion rotation, float scale)
                    : this(position, rotation, new float3(scale, scale, scale)) { }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var tf = gameObject.transform;
                var create = cPrev == null;
                var prev = create ? default : (Transform)cPrev;

                if (create || !prev.Position.Equals(Position)) {
                    tf.localPosition = Position;
                }
                if (create || !prev.Rotation.Equals(Rotation)) {
                    tf.localRotation = Rotation;
                }
                if (create || !prev.Scale.Equals(Scale)) {
                    tf.localScale = Scale;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) { }
        }
    }
}