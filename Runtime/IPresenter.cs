using Rondo.Core;
using Rondo.Core.Lib;
using UnityEngine;

namespace Rondo.Unity {
    public interface IPresenter {
        IMessenger Messenger { get; }
        Camera Camera { get; set; }
        void RequestObjRef(ObjRef objRef, GameObject gameObject, Ca<GameObject, GameObject> fn);
        internal void StoreRef(ObjRef objRef, GameObject gameObject);
        Settings Settings { get; }
    }
}