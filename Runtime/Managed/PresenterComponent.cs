using Rondo.Core.Lib;
using UnityEngine;

namespace Rondo.Unity.Managed {
    [DisallowMultipleComponent]
    internal class PresenterComponent : MonoBehaviour {
        private InputComponent _inputComponent;
        public Ca<IPresenter> OnFrameEnded;
        public IPresenter Presenter;

        private void Start() {
            _inputComponent = new GameObject($"{nameof(Rondo)}.{nameof(InputComponent)}", typeof(InputComponent)).GetComponent<InputComponent>();
            _inputComponent.transform.SetParent(transform, false);
            _inputComponent.Presenter = Presenter;
        }

        private void Update() {
            Lifecycle.TriggerUpdate(Time.deltaTime, Presenter);
        }

        private void LateUpdate() {
            OnFrameEnded.Invoke(Presenter);
        }

        private void OnDestroy() {
            Destroy(_inputComponent.gameObject);
        }
    }
}