using System;
using Rondo.Core.Memory;

namespace Rondo.Unity {
    public readonly unsafe struct Key : IEquatable<Key> {
        internal readonly Ptr Value;

        internal Key(Ptr value) {
            Value = value;
        }

        public T GetValue<T>() where T : unmanaged {
            return *Value.Cast<T>();
        }

        public static Key New<T>(T value) where T : unmanaged {
            return new Key(Mem.C.CopyPtr(value));
        }

        public bool Equals(Key other) {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj) {
            return obj is Key other && Equals(other);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public static bool operator ==(Key left, Key right) {
            return left.Equals(right);
        }

        public static bool operator !=(Key left, Key right) {
            return !(left == right);
        }
    }
}