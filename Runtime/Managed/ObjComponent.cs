using System;
using System.Collections.Generic;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity.Managed {
    [DisallowMultipleComponent]
    internal class ObjComponent : MonoBehaviour {
        private IntPtr _keyData;
        private int _keySize;
        private Ts _keyType;

        public Dictionary<S, GameObject> Children { get; } = new();

        public Key Key {
            get => new Key(Mem.C.CopyPtrFromOuterMemory(_keyType, _keyData));
            set => Mem.AllocAndCopyPtrToOuterMemory(value.Value, ref _keyData, ref _keySize, out _keyType);
        }

        private void OnDestroy() {
            Mem.FreeOuterMemory(ref _keyData);
        }
    }
}