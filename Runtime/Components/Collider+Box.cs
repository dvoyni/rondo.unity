// using Rondo.Core.Memory;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace Rondo.Unity.Components {
//     //TODO: rest colliders
//     //TODO: disable physics
//     public static unsafe partial class Collider {
//         public readonly struct BoxConfig {
//             public readonly float3 Center;
//             public readonly float3 Size;
//             public readonly bool IsTrigger;
//
//             public BoxConfig(float3 size, float3 center = default, bool isTrigger = true) {
//                 Center = center;
//                 Size = size;
//                 IsTrigger = isTrigger;
//             }
//         }
//
//         private static readonly ulong _idBox = CompExtensions.NextId;
//
//         public static Comp Box(BoxConfig config) {
//             return new Comp(_idBox, &SyncBox, Mem.C.CopyPtr(config));
//         }
//
//         private static void SyncBox(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
//             if (pPrev == pNext) {
//                 return;
//             }
//             if (pNext == Ptr.Null) {
//                 Utils.Helpers.DestroySafe<BoxCollider>(gameObject);
//                 return;
//             }
//             if (pPrev == Ptr.Null) {
//                 gameObject.AddComponent<BoxCollider>();
//             }
//
//             var collider = gameObject.GetComponent<BoxCollider>();
//             var prev = pPrev == Ptr.Null ? default : *pPrev.Cast<BoxConfig>();
//             var next = *pNext.Cast<BoxConfig>();
//
//             if (!prev.Center.Equals(next.Center)) {
//                 collider.center = next.Center;
//             }
//             if (!prev.Size.Equals(next.Size)) {
//                 collider.size = next.Size;
//             }
//             if (prev.IsTrigger != next.IsTrigger) {
//                 collider.isTrigger = next.IsTrigger;
//             }
//         }
//     }
// }