using System.Collections.Generic;
using UnityEngine;

namespace Rondo.Unity.Managed {
    [DisallowMultipleComponent]
    internal class ObjComponent : MonoBehaviour {
        public object Key;
        public Dictionary<string, ObjComponent> Children { get; } = new();
        public Obj Prev;
    }
}