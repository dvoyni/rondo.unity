using Rondo.Core.Extras;
using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity {
    public static class Lifecycle {
        public delegate void Update(float deltaTime, IPresenter presenter);
        public static event Update OnUpdate;

        public static void TriggerUpdate(float deltaTime, IPresenter presenter) {
            OnUpdate?.Invoke(deltaTime, presenter);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            Access.__DomainReload();
            Mem.Manager = new MemManager();
        }
#endif
    }
}