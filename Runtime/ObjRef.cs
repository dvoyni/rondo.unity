using System;

namespace Rondo.Unity {
    public readonly struct ObjRef : IEquatable<ObjRef> {
        private readonly ulong _id;

        private ObjRef(ulong id) {
            _id = id;
        }

        public static bool operator ==(ObjRef a, ObjRef b) {
            return a.Equals(b);
        }

        public static bool operator !=(ObjRef a, ObjRef b) {
            return !(a == b);
        }

        public bool Equals(ObjRef other) {
            return _id == other._id;
        }

        public override bool Equals(object obj) {
            return obj is ObjRef other && Equals(other);
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        private static ulong _lastId = 0;

        public static ObjRef New() {
            return new ObjRef(++_lastId);
        }

        public static ObjRef NoRef => new();
    }
}