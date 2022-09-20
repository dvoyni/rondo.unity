using System.Collections.Generic;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity.Components {
    public static unsafe partial class Rendering {
        private static readonly Ts _materialType = (Ts)typeof(Material);
        private static readonly List<Material[]> _matArrayCache = new();
        private static readonly List<Material> _tmpMaterials = new();

        public readonly struct MeshRendererConfig {
            public readonly L<S> MaterialAddresses;
            public readonly ShadowCastingMode CastShadows;
            public readonly bool StaticShadowCaster;
            public readonly LightProbeUsage LightProbes;
            public readonly ObjRef AnchorOverride;
            public readonly bool DynamicOcclusion;
            public readonly uint RenderingLayerMask;

            public MeshRendererConfig(
                L<S> materialAddresses,
                ShadowCastingMode castShadows = ShadowCastingMode.On,
                bool staticShadowCaster = false,
                LightProbeUsage lightProbes = LightProbeUsage.BlendProbes,
                ObjRef anchorOverride = default,
                bool dynamicOcclusion = true,
                uint renderingLayerMask = 1 << 0
            ) {
                MaterialAddresses = materialAddresses;
                CastShadows = castShadows;
                StaticShadowCaster = staticShadowCaster;
                LightProbes = lightProbes;
                DynamicOcclusion = dynamicOcclusion;
                RenderingLayerMask = renderingLayerMask;
                AnchorOverride = anchorOverride;
            }
        }

        private static readonly ulong _idMeshRenderer = CompExtensions.NextId;

        public static Comp MeshRenderer(MeshRendererConfig config) {
            return new(_idMeshRenderer, &SyncMeshRenderer, Mem.C.CopyPtr(config));
        }

        public static Comp MeshRenderer(string materialAddress, ShadowCastingMode castShadows = ShadowCastingMode.On) {
            return MeshRenderer(new MeshRendererConfig(
                materialAddresses: new((S)materialAddress),
                castShadows: castShadows));
        }

        private static void SyncMeshRenderer(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }

            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<MeshRenderer>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<MeshRenderer>();
            }

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            var force = pPrev == Ptr.Null;
            var prev = pPrev == Ptr.Null ? default : *pPrev.Cast<MeshRendererConfig>();
            var next = *pNext.Cast<MeshRendererConfig>();

            var r = prev.MaterialAddresses.Merge(
                next.MaterialAddresses,
                &RemoveMaterial, &SyncMaterial, &AddMaterial,
                new SyncMaterialData(gameObject, presenter)
            );
            if (r.Remove > 0) {
                meshRenderer.GetSharedMaterials(_tmpMaterials);

                for (var i = 0; i < r.Remove; i++) {
                    if (_tmpMaterials.Count > 0) {
                        _tmpMaterials.RemoveAt(_tmpMaterials.Count - 1);
                    }
                }
                meshRenderer.sharedMaterials = TmpMaterialsToArr();
            }

            if (force || (prev.CastShadows != next.CastShadows)) {
                meshRenderer.shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode)next.CastShadows;
            }
            if (force || (prev.StaticShadowCaster != next.StaticShadowCaster)) {
                meshRenderer.staticShadowCaster = next.StaticShadowCaster;
            }
            if (force || (prev.LightProbes != next.LightProbes)) {
                meshRenderer.lightProbeUsage = (UnityEngine.Rendering.LightProbeUsage)next.LightProbes;
            }
            if (next.AnchorOverride == ObjRef.NoRef) {
                if (prev.AnchorOverride != ObjRef.NoRef) {
                    meshRenderer.probeAnchor = null;
                }
            }
            else {
                presenter.RequestObjRef(next.AnchorOverride, gameObject, Ca.New<GameObject, GameObject>(&HandleAnchorRef));
            }
            if (force || (prev.DynamicOcclusion != next.DynamicOcclusion)) {
                meshRenderer.allowOcclusionWhenDynamic = next.DynamicOcclusion;
            }
            if (force || (prev.RenderingLayerMask != next.RenderingLayerMask)) {
                meshRenderer.renderingLayerMask = next.RenderingLayerMask;
            }
        }

        private static void HandleAnchorRef(GameObject gameObject, GameObject tfm) {
            gameObject.GetComponent<MeshRenderer>().probeAnchor = tfm.transform;
        }

        private static CLa<GameObject, Object> HandleMeshRendererMaterialLoaded(int index) {
            static void Impl(GameObject gameObject, Object material, int* index) {
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                meshRenderer.GetSharedMaterials(_tmpMaterials);
                var n = *index;
                while (_tmpMaterials.Count <= n) {
                    _tmpMaterials.Add(null);
                }
                _tmpMaterials[n] = (Material)material;

                meshRenderer.sharedMaterials = TmpMaterialsToArr();
            }

            return CLa.New<GameObject, Object, int>(&Impl, index);
        }

        private static Material[] TmpMaterialsToArr() {
            var arr = GetTmpMaterials(_tmpMaterials.Count);
            for (var i = 0; i < arr.Length; i++) {
                arr[i] = _tmpMaterials[i];
            }
            _tmpMaterials.Clear();
            return arr;
        }

        private readonly struct SyncMaterialData {
            public readonly GameObject GameObject;
            public readonly IPresenter Presenter;
            public readonly int Remove;

            public SyncMaterialData(GameObject gameObject, IPresenter presenter, int remove = 0) {
                GameObject = gameObject;
                Presenter = presenter;
                Remove = remove;
            }

            public SyncMaterialData NextRemove => new SyncMaterialData(GameObject, Presenter, Remove + 1);
        }

        private static SyncMaterialData RemoveMaterial(int index, S address, SyncMaterialData smd) {
            return smd.NextRemove;
        }

        private static SyncMaterialData SyncMaterial(int index, S prev, S next, SyncMaterialData smd) {
            if (prev != next) {
                AddressablesCache.Load(next, _materialType, smd.GameObject, HandleMeshRendererMaterialLoaded(index));
            }
            return smd;
        }

        private static SyncMaterialData AddMaterial(int index, S address, SyncMaterialData smd) {
            AddressablesCache.Load(address, _materialType, smd.GameObject, HandleMeshRendererMaterialLoaded(index));
            return smd;
        }

        private static Material[] GetTmpMaterials(int length) {
            while (_matArrayCache.Count <= length) {
                _matArrayCache.Add(new Material[_matArrayCache.Count]);
            }
            return _matArrayCache[length];
        }

        public enum ShadowCastingMode {
            Off,
            On,
            TwoSided,
            ShadowsOnly,
        }

        public enum LightProbeUsage {
            Off = 0,
            BlendProbes = 1,
            UseProxyVolume = 2,
            CustomProvided = 4,
        }
    }
}