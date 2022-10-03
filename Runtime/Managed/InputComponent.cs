using System;
using System.Collections.Generic;
using Rondo.Core.Lib.Containers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using Input = Rondo.Unity.Subs.Input;

namespace Rondo.Unity.Managed {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]
    internal class InputComponent : MonoBehaviour {
        private static readonly RaycastHitComparer _raycastHitComparer = new();

        private float2 _prevMousePosition;
        private Input.PointerEventButton _pressed;
        private Input.PointerEventButton _dragStarted;

        private readonly RaycastHit[] _raycastResults = new RaycastHit[32];

        [NonSerialized] public IPresenter Presenter;

        private void Update() {
            var passClick =
                    !EventSystem.current.IsPointerOverGameObject(0)
                    && (
                        (UnityEngine.Input.touchCount == 0)
                        || !EventSystem.current.IsPointerOverGameObject(UnityEngine.Input.GetTouch(0).fingerId)
                    )
                    && !EventSystem.current.IsPointerOverGameObject(-1);
            //for android u should put these if statement only in "touch began" as said in unity manual"
            //&& (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began);

            var pos = ((float3)UnityEngine.Input.mousePosition).xy;
            var keys = ObjKeysUnderPointer(pos);

            if (passClick) {
                if (UnityEngine.Input.GetMouseButtonDown(0)) {
                    _pressed |= Input.PointerEventButton.Left;
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Down, Input.PointerEventButton.Left, pos, keys
                    ));
                }
                if (UnityEngine.Input.GetMouseButtonDown(1)) {
                    _pressed |= Input.PointerEventButton.Right;
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Down, Input.PointerEventButton.Right, pos, keys
                    ));
                }
                if (UnityEngine.Input.GetMouseButtonDown(1)) {
                    _pressed |= Input.PointerEventButton.Middle;
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Down, Input.PointerEventButton.Middle, pos, keys
                    ));
                }
            }

            var d = _prevMousePosition - pos;
            if (math.dot(d, d) > 0.5f) {
                _prevMousePosition = pos;

                if (_dragStarted != _pressed) {
                    var newDrag = _pressed & ~_dragStarted;
                    if ((newDrag & Input.PointerEventButton.Left) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.BeginDrag, Input.PointerEventButton.Left, pos, keys
                        ));
                    }
                    if ((newDrag & Input.PointerEventButton.Right) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.BeginDrag, Input.PointerEventButton.Right, pos, keys
                        ));
                    }
                    if ((newDrag & Input.PointerEventButton.Middle) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.BeginDrag, Input.PointerEventButton.Middle, pos, keys
                        ));
                    }
                    _dragStarted = _pressed;
                }

                if ((_dragStarted & Input.PointerEventButton.Left) != 0) {
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Drag, Input.PointerEventButton.Left, pos, keys
                    ));
                }
                if ((_dragStarted & Input.PointerEventButton.Right) != 0) {
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Drag, Input.PointerEventButton.Right, pos, keys
                    ));
                }
                if ((_dragStarted & Input.PointerEventButton.Middle) != 0) {
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Drag, Input.PointerEventButton.Middle, pos, keys
                    ));
                }

                if (Presenter.Settings.TriggerPointerMoveEvent) {
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Move, Input.PointerEventButton.None, pos, keys
                    ));
                }
            }

            if (passClick) {
                if (UnityEngine.Input.GetMouseButtonUp(0)) {
                    _pressed &= ~Input.PointerEventButton.Left;
                    if ((_dragStarted & Input.PointerEventButton.Left) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.EndDrag, Input.PointerEventButton.Left, pos, keys
                        ));
                    }
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Up, Input.PointerEventButton.Left, pos, keys
                    ));
                    if ((_dragStarted & Input.PointerEventButton.Left) == 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.Click, Input.PointerEventButton.Left, pos, keys
                        ));
                    }
                    _dragStarted &= ~Input.PointerEventButton.Left;
                }
                if (UnityEngine.Input.GetMouseButtonUp(1)) {
                    _pressed &= ~Input.PointerEventButton.Right;
                    if ((_dragStarted & Input.PointerEventButton.Right) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.EndDrag, Input.PointerEventButton.Right, pos, keys
                        ));
                    }
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Up, Input.PointerEventButton.Right, pos, keys
                    ));
                    if ((_dragStarted & Input.PointerEventButton.Right) == 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.Click, Input.PointerEventButton.Right, pos, keys
                        ));
                    }
                    _dragStarted &= ~Input.PointerEventButton.Right;
                }
                if (UnityEngine.Input.GetMouseButtonUp(1)) {
                    _pressed &= ~Input.PointerEventButton.Middle;
                    if ((_dragStarted & Input.PointerEventButton.Middle) != 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.EndDrag, Input.PointerEventButton.Middle, pos, keys
                        ));
                    }
                    Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                        Input.PointerEventKind.Up, Input.PointerEventButton.Middle, pos, keys
                    ));
                    if ((_dragStarted & Input.PointerEventButton.Middle) == 0) {
                        Presenter.Messenger.TriggerSub(new Input.PointerEventData(
                            Input.PointerEventKind.Click, Input.PointerEventButton.Middle, pos, keys
                        ));
                    }
                    _dragStarted &= ~Input.PointerEventButton.Middle;
                }
            }
        }
#if UNITY_STANDALONE
#endif

        private L<Key> ObjKeysUnderPointer(float2 pos) {
            var cam = Presenter.Camera;
            var result = new L<Key>();
            if (!cam) {
                cam = Presenter.Camera = Camera.main;
            }
            if (!ReferenceEquals(cam, null)) {
                var ray = cam.ScreenPointToRay(new float3(pos, 0));
                var hits = Physics.RaycastNonAlloc(ray, _raycastResults, cam.farClipPlane);
                Array.Sort(_raycastResults, 0, hits, _raycastHitComparer);
                for (var i = 0; i < hits; i++) {
                    var hit = _raycastResults[i];
                    var oc = hit.collider.GetComponent<ObjComponent>();
                    if (!ReferenceEquals(oc, null) && (oc.Key != default)) {
                        result = oc.Key + result;
                    }
                    //check prefab parent
                    var parent = hit.transform.parent;
                    if (!ReferenceEquals(hit.transform.parent, null)) {
                        oc = parent.GetComponent<ObjComponent>();
                        if (!ReferenceEquals(oc, null) && (oc.Children.Count == 0) && (oc.Key != default)) {
                            result = oc.Key + result;
                        }
                    }
                }
            }
            return result;
        }

        private class RaycastHitComparer : IComparer<RaycastHit> {
            public int Compare(RaycastHit x, RaycastHit y) {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}