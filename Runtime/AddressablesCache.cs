using System;
using System.Collections.Generic;
using System.Reflection;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Rondo.Unity {
    public class AddressablesCache {
        private delegate void LoadDelegate(S address);

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            _cache.Clear();
            _loading.Clear();
            _pending.Clear();
        }
#endif

        private static readonly Dictionary<(S, Ts), Object> _cache = new();
        private static readonly HashSet<(S, Ts)> _loading = new();
        private static readonly Dictionary<PendingKey, Pending> _pending = new();
        private static readonly List<PendingKey> _tmpKeys = new();

        //TODO: make special case for GameObject with pooling
        /// <summary>
        /// Disposes closure when loaded
        /// </summary>
        public static void Load(S address, Ts type, GameObject gameObject, Xa<GameObject, Object> fn) {
            var x = (address, type);
            if (_cache.TryGetValue(x, out var obj)) {
                fn.Invoke(gameObject, obj);
                fn.Dispose();
                return;
            }

            _pending[new PendingKey(gameObject, fn)] = new Pending(address, type, gameObject, fn);
            if (_loading.Contains(x)) {
                return;
            }

            _loading.Add(x);
            typeof(AddressablesCache)
                    .GetMethod(nameof(LoadT), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod((Type)type)
                    .CreateDelegate(typeof(LoadDelegate))
                    .DynamicInvoke(address);
        }

        private static void LoadT<T>(S address)
                where T : Object {
            var op = Addressables.LoadAssetAsync<T>((string)address);
            op.Completed += handle => { //TODO: reduce allocation as much as possible
                var type = (Ts)typeof(T);
                var x = (address, type);
                _loading.Remove(x);
                _cache[x] = handle.Result;
                foreach (var (k, v) in _pending) {
                    if ((v.Address == address) && (v.Type == type)) {
                        v.Fn.Invoke(v.GameObject, handle.Result);
                        v.Fn.Dispose();
                        _tmpKeys.Add(k);
                    }
                }
                foreach (var it in _tmpKeys) {
                    _pending.Remove(it);
                }
                _tmpKeys.Clear();
            };
        }

        public static void Clear() {
            _cache.Clear();
        }

        private readonly struct Pending {
            public readonly GameObject GameObject;
            public readonly Xa<GameObject, Object> Fn;
            public readonly S Address;
            public readonly Ts Type;

            public Pending(S address, Ts type, GameObject gameObject, Xa<GameObject, Object> fn) {
                GameObject = gameObject;
                Fn = fn;
                Address = address;
                Type = type;
            }
        }

        private readonly struct PendingKey : IEquatable<PendingKey> {
            public readonly GameObject GameObject;
            public readonly Xa<GameObject, Object> Fn;

            public PendingKey(GameObject gameObject, Xa<GameObject, Object> fn) {
                GameObject = gameObject;
                Fn = fn;
            }

            public bool Equals(PendingKey other) {
                return Equals(GameObject, other.GameObject) && Fn.Equals(other.Fn);
            }

            public override bool Equals(object obj) {
                return obj is PendingKey other && Equals(other);
            }

            public override int GetHashCode() {
                return (GameObject != null ? GameObject.GetHashCode() : 0);
            }
        }
    }
}