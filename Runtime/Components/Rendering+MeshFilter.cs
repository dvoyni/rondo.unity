using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;

namespace Rondo.Unity.Components {
    //TODO: procedural meshes support
    public static unsafe partial class Rendering {
        private static readonly Ts _meshType = Ts.OfUnmanaged(typeof(Mesh));

        public static readonly quaternion FromBlender = quaternion.Euler(-math.PI / 2, 0, math.PI);

        public readonly struct MeshFilterConfig {
            public readonly S MeshAddress;

            public MeshFilterConfig(S meshAddress) {
                MeshAddress = meshAddress;
            }
        }

        private static readonly ulong _idMeshFilter = CompExtensions.NextId;

        public static Comp MeshFilter(MeshFilterConfig config) {
            return new Comp(_idMeshFilter, &SyncMeshFilter, Mem.C.CopyPtr(config));
        }

        public static Comp MeshFilter(string address) {
            return MeshFilter(new MeshFilterConfig(meshAddress: (S)address));
        }

        private static void SyncMeshFilter(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<MeshFilter>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<MeshFilter>();
            }
            var force = pPrev != Ptr.Null;
            var prev = force ? default : *pPrev.Cast<MeshFilterConfig>();
            var next = *pNext.Cast<MeshFilterConfig>();

            if (force || (prev.MeshAddress != next.MeshAddress)) {
                if (next.MeshAddress == S.Empty) {
                    gameObject.GetComponent<MeshFilter>().sharedMesh = null;
                }
                else {
                    AddressablesCache.Load(next.MeshAddress, _meshType, gameObject, Xa.New<GameObject, Object>(&HandleMeshFilterMeshLoaded));
                }
            }
        }

        private static void HandleMeshFilterMeshLoaded(GameObject gameObject, Object mesh) {
            gameObject.GetComponent<MeshFilter>().sharedMesh = (Mesh)mesh;
        }
    }
}