using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity {
    public readonly unsafe struct Comp {
        /// <summary>
        /// Prev is Ptr.Null means component should created
        /// Next is Ptr.Null means component should be deleted
        /// </summary>
        public readonly delegate*<IPresenter, GameObject, Ptr /*prev model*/, Ptr /*next model*/, void> Sync;
        public readonly Ptr Data;
        public readonly ulong Id;

        public Comp(ulong id, delegate*<IPresenter, GameObject, Ptr, Ptr, void> sync, Ptr data) {
            Id = id;
            Sync = sync;
            Data = data;
        }

#if DEBUG
        public override string ToString() {
            return Serializer.Stringify(this);
        }
#endif
    }

    public static class CompExtensions {
        private static ulong _lastId = 0;
        public static ulong NextId => ++_lastId;
    }
}