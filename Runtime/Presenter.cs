using System;
using System.Collections.Generic;
using Rondo.Core;
using Rondo.Unity.Managed;
using UnityEngine;

// ReSharper disable ParameterHidesMember
// ReSharper disable LocalVariableHidesMember
namespace Rondo.Unity {
    [DisallowMultipleComponent]
    public class Presenter : MonoBehaviour, IPresenter {
        private class DummyMessageReceiver : IMessageReceiver {
            public void PostMessage(IMsg msg) { }

            public void TriggerSub<T>(T eventData) { }
        }

        private readonly List<RefRequest> _refRequest = new();
        private readonly Dictionary<ObjRef, GameObject> _refResolve = new();
        private IMessageReceiver _messageReceiver = new DummyMessageReceiver();
        private PresenterSettings _settings;

        private GameObject _root;
        private Obj _next;
        private bool _presentRequired;

        public static Presenter Create(PresenterSettings settings = default) {
            var go = new GameObject($"{nameof(Rondo)}.{nameof(Presenter)}", typeof(Presenter), typeof(InputComponent));

            var presenter = go.GetComponent<Presenter>();
            presenter._settings = settings;
            presenter._root = new GameObject("", typeof(ObjComponent));
            presenter._root.transform.SetParent(presenter.transform, false);

            return presenter;
        }

        public delegate void UpdateDelegate(float deltaTime, IPresenter presenter);
        public static event UpdateDelegate OnFixedUpdate;
        public static event UpdateDelegate OnFrame;

        void IPresenter<Obj>.Present(Obj next, IMessageReceiver messageReceiver) {
            _messageReceiver = messageReceiver;
            _next = next;
            _presentRequired = true;
        }

        PresenterSettings IPresenter.Settings => _settings;

        Camera IPresenter.Camera { get; set; }

        void IPresenter.RequestObjRef(ObjRef objRef, Action<GameObject> fn) {
            _refRequest.Add(new RefRequest(objRef, fn));
        }

        void IPresenter.StoreRef(ObjRef objRef, GameObject gameObject) {
            _refResolve[objRef] = gameObject;
        }

        IMessageReceiver IPresenter.MessageReceiver => _messageReceiver;

        private void Awake() {
            foreach (var presenter in FindObjectsOfType<Presenter>()) {
                if (presenter != this) {
                    Destroy(presenter.gameObject);
                }
            }
        }

        private void FixedUpdate() {
            OnFixedUpdate?.Invoke(Time.fixedDeltaTime, this);
        }

        private void Update() {
            OnFrame?.Invoke(Time.deltaTime, this);
        }

        private void LateUpdate() {
            if (_presentRequired) {
                _presentRequired = false;
                _refRequest.Clear();
                _refResolve.Clear();
                SyncGameObject(_next, _root.GetComponent<ObjComponent>());
                foreach (var r in _refRequest) {
                    r.Fn.Invoke(_refResolve.TryGetValue(r.ObjRef, out var target) ? target : null);
                }
                Obj.LastIndex = 0;
            }
        }

        private void SyncGameObject(Obj next, ObjComponent obj) {
            var gameObject = obj.gameObject;
            var prev = obj.Prev;

            if (prev.Inactive != next.Inactive) {
                gameObject.SetActive(!next.Inactive);
            }

            if (prev.Name != next.Name) {
                gameObject.name = next.Name;
            }

            if (prev.Static != next.Static) {
                gameObject.isStatic = next.Static;
            }

            if (prev.Key != next.Key) {
                gameObject.GetComponent<ObjComponent>().Key = next.Key;
            }

            if (next.Ref != ObjRef.NoRef) {
                ((IPresenter)this).StoreRef(next.Ref, gameObject);
            }

            SyncComponents(prev.Components, next.Components, obj.gameObject);
            SyncChildren(next, obj);
            obj.Prev = next;
        }

        private void SyncComponents(
            IReadOnlyDictionary<Type, IComp> prev, IReadOnlyDictionary<Type, IComp> next, GameObject gameObject
        ) {
            foreach (var (type, p) in prev) {
                if (!next.ContainsKey(type)) {
                    p.Remove(this, gameObject);
                }
            }

            foreach (var (type, n) in next) {
                prev.TryGetValue(type, out var p);
                if (!n.Equals(p)) {
                    n.Sync(this, gameObject, p);
                }
            }
        }

        private void SyncChildren(Obj next, ObjComponent obj) {
            foreach (var (name, n) in next.Children) {
                if (!obj.Children.TryGetValue(name, out var child)) {
                    obj.Children[name] = child =
                            new GameObject(name, typeof(ObjComponent)).GetComponent<ObjComponent>();
                    child.transform.SetParent(obj.gameObject.transform, false);
                }

                SyncGameObject(n, child);
            }

            foreach (var (name, _) in obj.Prev.Children) {
                if (!next.Children.ContainsKey(name)) {
                    if (obj.Children.Remove(name, out var child)) {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private readonly struct RefRequest {
            public readonly ObjRef ObjRef;
            public readonly Action<GameObject> Fn;

            public RefRequest(ObjRef objRef, Action<GameObject> fn) {
                ObjRef = objRef;
                Fn = fn;
            }
        }
    }
}