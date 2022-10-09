using System.Collections.Generic;
using Rondo.Core;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Managed;
using UnityEngine;

namespace Rondo.Unity {
    public unsafe class Presenter<TMsg> : IPresenter<Obj>, IPresenter where TMsg : unmanaged {
        private readonly GameObject _root;
        private readonly PresenterComponent _presenterComponent;
        private readonly List<RefRequest> _refRequest = new();
        private readonly Dictionary<ObjRef, GameObject> _refResolve = new();

        private Obj _prev;
        private Obj _next;
        private bool _presentRequired;
        private readonly Settings _settings;

        public Presenter(Transform rootTransform = null, Settings settings = default) {
            _settings = settings;
            _root = new GameObject("", typeof(ObjComponent));
            _root.transform.SetParent(rootTransform, false);
            _presenterComponent = new GameObject($"{nameof(Rondo)}.{nameof(PresenterComponent)}", typeof(PresenterComponent))
                    .GetComponent<PresenterComponent>();
            _presenterComponent.OnFrameEnded = Ca.New<IPresenter>(&HandleFrameEnded);
            _presenterComponent.Presenter = this;
        }

        public IMessenger Messenger { get; set; }

        public Camera Camera { get; set; }

        public void RequestObjRef(ObjRef objRef, GameObject gameObject, Ca<GameObject, GameObject> fn) {
            _refRequest.Add(new RefRequest(objRef, gameObject, fn));
        }

        void IPresenter.StoreRef(ObjRef objRef, GameObject gameObject) {
            _refResolve[objRef] = gameObject;
        }

        public Settings Settings => _settings;

        void IPresenter<Obj>.Present(Obj next) {
            _next = next;
            _presentRequired = true;
            _prev = Serializer.Clone(_prev);
        }

        private static void HandleFrameEnded(IPresenter presenter) {
            ((Presenter<TMsg>)presenter).HandleFrameEnded();
        }

        private void HandleFrameEnded() {
            if (_presentRequired) {
                _presentRequired = false;
                _refRequest.Clear();
                _refResolve.Clear();
                SyncGameObject(_prev, _next, new SyncData(this, _root));
                foreach (var r in _refRequest) {
                    if (_refResolve.TryGetValue(r.ObjRef, out var target)) {
                        r.Fn.Invoke(r.GameObject, target);
                    }
                    else {
                        r.Fn.Invoke(r.GameObject, null);
                    }
                }
                _prev = _next;
                Obj._lastIndex = 0;
            }
        }

        private static void SyncChildren(Obj prev, Obj next, SyncData sd) {
            prev.Children.Merge(next.Children, &RemoveChild, &SyncChild, &AddChild, sd);
        }

        private static SyncData AddChild(S name, Obj next, SyncData sd) {
            return SyncChild(name, new Obj(), next, sd);
        }

        private static SyncData SyncChild(S name, Obj prev, Obj next, SyncData sd) {
            if (!sd.ObjComponent.Children.TryGetValue(name, out var child)) {
                sd.ObjComponent.Children[name] = child = new GameObject((string)name, typeof(ObjComponent));
                child.transform.SetParent(sd.GameObject.transform, false);
            }

            SyncGameObject(prev, next, new SyncData(sd.Presenter, child));
            return sd;
        }

        private static SyncData RemoveChild(S name, Obj prev, SyncData sd) {
            if (sd.ObjComponent.Children.Remove(name, out var gameObject)) {
                Object.Destroy(gameObject);
            }
            return sd;
        }

        private static void SyncGameObject(Obj prev, Obj next, SyncData sd) {
            if (prev.Inactive != next.Inactive) {
                sd.GameObject.SetActive(!next.Inactive);
            }

            if (prev.Name != next.Name) {
                sd.GameObject.name = (string)next.Name;
            }

            if (prev.Static != next.Static) {
                sd.GameObject.isStatic = next.Static;
            }

            if (prev.Key != next.Key) {
                sd.GameObject.GetComponent<ObjComponent>().Key = next.Key;
            }

            if (next.Ref != ObjRef.NoRef) {
                sd.Presenter.StoreRef(next.Ref, sd.GameObject);
            }

            SyncComponents(prev.Components, next.Components, sd);
            SyncChildren(prev, next, new SyncData(sd.Presenter, sd.GameObject));
        }

        private static void SyncComponents(D<ulong, Comp> prev, D<ulong, Comp> next, SyncData sd) {
            prev.Merge(next, &RemoveComponent, &UpdateComponent, &AddComponent, sd);
        }

        private static SyncData AddComponent(ulong _, Comp next, SyncData sd) {
            next.Sync(sd.Presenter, sd.GameObject, Ptr.Null, next.Data);
            return sd;
        }

        private static SyncData UpdateComponent(ulong _, Comp prev, Comp next, SyncData sd) {
            next.Sync(sd.Presenter, sd.GameObject, prev.Data, next.Data);
            return sd;
        }

        private static SyncData RemoveComponent(ulong _, Comp prev, SyncData sd) {
            prev.Sync(sd.Presenter, sd.GameObject, prev.Data, Ptr.Null);
            return sd;
        }

        private readonly struct SyncData {
            public readonly IPresenter Presenter;
            public readonly GameObject GameObject;
            public readonly ObjComponent ObjComponent;

            public SyncData(IPresenter presenter, GameObject gameObject) {
                Presenter = presenter;
                GameObject = gameObject;
                ObjComponent = gameObject.GetComponent<ObjComponent>();
            }
        }

        private readonly struct RefRequest {
            public readonly ObjRef ObjRef;
            public readonly GameObject GameObject;
            public readonly Ca<GameObject, GameObject> Fn;

            public RefRequest(ObjRef objRef, GameObject gameObject, Ca<GameObject, GameObject> fn) {
                ObjRef = objRef;
                GameObject = gameObject;
                Fn = fn;
            }
        }

        public void Dispose() {
            if (_root) {
                Object.Destroy(_root);
            }
            if (_presenterComponent) {
                Object.Destroy(_presenterComponent.gameObject);
            }
        }
    }
}