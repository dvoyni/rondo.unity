using System;
using Rondo.Core;
using UnityEngine;

namespace Rondo.Unity {
    public interface IPresenter : IPresenter<Obj> {
        PresenterSettings Settings { get; }
        Camera Camera { get; set; }
        void RequestObjRef(ObjRef objRef, Action<GameObject> fn);
        internal void StoreRef(ObjRef objRef, GameObject gameObject);
        IMessageReceiver MessageReceiver { get; }
    }
}