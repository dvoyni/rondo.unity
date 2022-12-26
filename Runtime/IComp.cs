using UnityEngine;

namespace Rondo.Unity {
    public interface IComp {
        void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev);
        void Remove(IPresenter presenter, GameObject gameObject);
    }
}