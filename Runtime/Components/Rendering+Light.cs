// using Rondo.Core.Memory;
// using Rondo.Unity.Utils;
// using Unity.Mathematics;
// using UnityEngine;
// #if RONDO_URP
// using UnityEngine.Rendering.Universal;
// #endif
//
// namespace Rondo.Unity.Components {
//     public static unsafe partial class Rendering {
//         public struct LightConfig {
//             public LightType Type;
//             public bool UseColorTemperature;
//             public float4 Color;
//             public float ColorTemperature;
//             public float Intensity;
//             public float IndirectMultiplier;
//             public float Range;
//             public float SpotAngleInner;
//             public float SpotAngleOuter;
//             public LightRenderMode RenderMode;
//             public int CullingMask;
//             public LightShadows Shadows;
//             public float ShadowsStrength;
//             public float ShadowsBias;
//             public float ShadowsNormalBias;
//             public float ShadowsNearPlane;
//
//             public LightConfig(
//                 LightType type,
//                 float4 color = default,
//                 bool useColorTemperature = true,
//                 float colorTemperature = 5000,
//                 float intensity = 2,
//                 float indirectMultiplier = 1,
//                 float range = 10,
//                 float spotAngleInner = 21.8f,
//                 float spotAngleOuter = 30,
//                 LightRenderMode renderMode = LightRenderMode.Auto,
//                 int cullingMask = ~0,
//                 LightShadows shadows = LightShadows.Soft,
//                 float shadowsStrength = 0.5f,
//                 float shadowsBias = 0.05f,
//                 float shadowsNormalBias = 0.4f,
//                 float shadowsNearPlane = 0.2f
//             ) {
//                 Type = type;
//                 UseColorTemperature = useColorTemperature;
//                 Color = color;
//                 ColorTemperature = colorTemperature;
//                 Intensity = intensity;
//                 IndirectMultiplier = indirectMultiplier;
//                 Range = range;
//                 SpotAngleInner = spotAngleInner;
//                 SpotAngleOuter = spotAngleOuter;
//                 RenderMode = renderMode;
//                 CullingMask = cullingMask;
//                 Shadows = shadows;
//                 ShadowsStrength = shadowsStrength;
//                 ShadowsBias = shadowsBias;
//                 ShadowsNormalBias = shadowsNormalBias;
//                 ShadowsNearPlane = shadowsNearPlane;
//             }
//         }
//
//         private static readonly ulong _idLight = CompExtensions.NextId;
//
//         public static Comp Light(LightConfig config) {
//             return new Comp(_idLight, &SyncLight, Mem.C.CopyPtr(config));
//         }
//
//         private static void SyncLight(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
//             if (pPrev == pNext) {
//                 return;
//             }
//             if (pNext == Ptr.Null) {
// #if RONDO_URP
//                 Utils.Helpers.DestroySafe<UniversalAdditionalLightData>(gameObject);
// #endif
//                 Utils.Helpers.DestroySafe<Light>(gameObject);
//                 return;
//             }
//             if (pPrev == Ptr.Null) {
//                 gameObject.AddComponent<Light>();
//             }
//
//             var light = gameObject.GetComponent<Light>();
//             var force = pPrev == Ptr.Null;
//             var prev = force ? default : *pPrev.Cast<LightConfig>();
//             var next = *pNext.Cast<LightConfig>();
//
//             if (force || (prev.Type != next.Type)) {
//                 light.type = (UnityEngine.LightType)next.Type;
//             }
//             if (force || (prev.UseColorTemperature != next.UseColorTemperature)) {
//                 light.useColorTemperature = next.UseColorTemperature;
//             }
//             if (force || !prev.Color.Equals(next.Color)) {
//                 light.color = Colors.FromFloat4(next.Color);
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.ColorTemperature != next.ColorTemperature)) {
//                 light.colorTemperature = next.ColorTemperature;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.Intensity != next.Intensity)) {
//                 light.intensity = next.Intensity;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.IndirectMultiplier != next.IndirectMultiplier)) {
//                 light.bounceIntensity = next.IndirectMultiplier;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.Range != next.Range)) {
//                 light.range = next.Range;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.SpotAngleInner != next.SpotAngleInner)) {
//                 light.innerSpotAngle = next.SpotAngleInner;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.SpotAngleOuter != next.SpotAngleOuter)) {
//                 light.spotAngle = next.SpotAngleOuter;
//             }
//             if (force || (prev.RenderMode != next.RenderMode)) {
//                 light.renderMode = (UnityEngine.LightRenderMode)next.RenderMode;
//             }
//             if (force || (prev.CullingMask != next.CullingMask)) {
//                 light.cullingMask = next.CullingMask;
//             }
//             if (force || (prev.Shadows != next.Shadows)) {
//                 light.shadows = (UnityEngine.LightShadows)next.Shadows;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.ShadowsStrength != next.ShadowsStrength)) {
//                 light.shadowStrength = next.ShadowsStrength;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.ShadowsBias != next.ShadowsBias)) {
//                 light.shadowBias = next.ShadowsBias;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.ShadowsNormalBias != next.ShadowsNormalBias)) {
//                 light.shadowNormalBias = next.ShadowsNormalBias;
//             }
//             // ReSharper disable once CompareOfFloatsByEqualityOperator
//             if (force || (prev.ShadowsNearPlane != next.ShadowsNearPlane)) {
//                 light.shadowNearPlane = next.ShadowsNearPlane;
//             }
//         }
//
//         public enum LightRenderMode {
//             Auto,
//             ForcePixel,
//             ForceVertex,
//         }
//
//         public enum LightShadows {
//             None,
//             Hard,
//             Soft,
//         }
//
//         public enum LightType {
//             Spot = 0,
//             Directional = 1,
//             Point = 2,
//         }
//     }
// }