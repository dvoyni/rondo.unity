// using System;
// using Rondo.Core;
// using Rondo.Core.Extras;
// using Rondo.Core.Lib;
// using Rondo.Core.Memory;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace Rondo.Unity.Components {
//     public interface IDataDrivenComponent<in TComponentModel>
//             where TComponentModel : unmanaged, IEquatable<TComponentModel> {
//         IMessageReceiver<> MessageReceiver { set; }
//         void Sync(TComponentModel componentModel);
//     }
//
//     /// <summary>
//     /// Obj with Prefab Comp cannot have any child Obj
//     /// </summary>
//     public static unsafe class Prefab {
//         private static readonly Ts _gameObjectType = (Ts)typeof(GameObject);
//
//         public struct StaticComponentModel : IEquatable<StaticComponentModel> {
//             public bool Equals(StaticComponentModel other) {
//                 return true;
//             }
//         }
//
//         private readonly struct PrefabConfig<TComponentModel>
//                 where TComponentModel : unmanaged, IEquatable<TComponentModel> {
//             public readonly S Address;
//             public readonly TComponentModel ComponentModel;
//             public readonly bool KeepTf;
//
//             public PrefabConfig(
//                 S address,
//                 TComponentModel componentModel = default,
//                 bool keepTf = false
//             ) {
//                 Address = address;
//                 ComponentModel = componentModel;
//                 KeepTf = keepTf;
//             }
//         }
//
//         // ReSharper disable once StaticMemberInGenericType
//         private static readonly RefHash _refs = new();
//         private static readonly ulong _idPrefab = CompExtensions.NextId;
//
//         public static Comp WithData<TComponentModel>(string address, TComponentModel config, bool keepTransform = false)
//                 where TComponentModel : unmanaged, IEquatable<TComponentModel> {
//             return new Comp(_idPrefab, &SyncPrefab<TComponentModel>, Mem.C.CopyPtr(
//                 new PrefabConfig<TComponentModel>((S)address, config, keepTransform)
//             ));
//         }
//
//         public static Comp Static(string address, bool keepTransform = false) {
//             return WithData(address, new StaticComponentModel(), keepTransform);
//         }
//
//         private static void SyncPrefab<TComponentModel>(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) where TComponentModel : unmanaged, IEquatable<TComponentModel> {
//             if (pPrev == pNext) {
//                 return;
//             }
//
//             if (pNext == Ptr.Null) {
//                 var tf = gameObject.transform;
//                 if (tf.childCount > 0) {
//                     Object.Destroy(tf.GetChild(0).gameObject);
//                 }
//                 //TODO: investigate this is strange
//                 AddressablesCache.Load(default, _gameObjectType, gameObject, HandlePrefabLoad<TComponentModel>(default, _refs.Hash(presenter)));
//                 return;
//             }
//
//             var force = pPrev == Ptr.Null;
//             var prev = force ? default : *pPrev.Cast<PrefabConfig<TComponentModel>>();
//             var next = *pNext.Cast<PrefabConfig<TComponentModel>>();
//
//             if (force || (prev.Address != next.Address)) {
//                 AddressablesCache.Load(next.Address, _gameObjectType, gameObject,
//                     HandlePrefabLoad(next, _refs.Hash(presenter)));
//                 return;
//             }
//
//             //TODO: optimize transform access
//             if (typeof(TComponentModel) != typeof(StaticComponentModel) && gameObject.transform.childCount > 0) {
//                 if (!prev.ComponentModel.Equals(next.ComponentModel)) {
//                     gameObject.transform.GetChild(0).gameObject.GetComponent<IDataDrivenComponent<TComponentModel>>().Sync(next.ComponentModel);
//                 }
//             }
//         }
//
//         private static Xa<GameObject, Object> HandlePrefabLoad<TComponentModel>(PrefabConfig<TComponentModel> config, Ref presenterRef)
//                 where TComponentModel : unmanaged, IEquatable<TComponentModel> {
//             static void Impl(GameObject gameObject, Object obj, PrefabConfig<TComponentModel>* config, Ref* presenterRef) {
//                 var instance = (GameObject)Object.Instantiate(obj);
//                 var tf = instance.transform;
//                 if (!config->KeepTf) {
//                     tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
//                     tf.localScale = Vector3.one;
//                 }
//                 tf.SetParent(gameObject.transform, false);
//
//                 var messenger = _refs.Get<IPresenter>(*presenterRef).Runtime;
//                 if (typeof(TComponentModel) != typeof(StaticComponentModel)) {
//                     var c = instance.GetComponent<IDataDrivenComponent<TComponentModel>>();
//                     Assert.NotNull(c, $"Prefab should contain a component that implements {nameof(IDataDrivenComponent<TComponentModel>)}");
//
//                     c.MessageReceiver = messenger;
//                     c.Sync(config->ComponentModel);
//                 }
//             }
//
//             return Xa.New<GameObject, Object, PrefabConfig<TComponentModel>, Ref>(&Impl, config, presenterRef);
//         }
//     }
// }