using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Rondo.Unity {
    public class AddressablesCache {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            _cache.Clear();
            _loading.Clear();
            _pending.Clear();
        }
#endif

        private static readonly Dictionary<(string, Type), Object> _cache = new();
        private static readonly HashSet<(string, Type)> _loading = new();
        private static readonly List<(string, Type, Action<Object>)> _pending = new();

        //TODO: make special case for GameObject with pooling
        public static void Load<T>(string address, Action<Object> loaded) where T : Object {
            var type = typeof(T);
            var x = (address, type);
            if (_cache.TryGetValue(x, out var obj)) {
                loaded(obj);
                return;
            }

            _pending.Add((address, type, loaded));
            if (_loading.Contains(x)) {
                return;
            }

            _loading.Add(x);
            var op = Addressables.LoadAssetAsync<T>(address);
            op.Completed += handle => {
                _loading.Remove(x);
                _cache[x] = handle.Result;
                for (var index = _pending.Count - 1; index >= 0; index--) {
                    var (a, t, callback) = _pending[index];
                    if ((a == address) && (t == type)) {
                        callback(handle.Result);
                        _pending[index] = _pending[^1];
                        _pending.RemoveAt(_pending.Count - 1);
                    }
                }
            };
        }

        public static void Clear() {
            _cache.Clear();
        }
    }
}