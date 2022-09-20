using Rondo.Core;
using Rondo.Core.Extras;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using Rondo.Unity.Subs;
using UnityEngine;

namespace Rondo.Unity {
    public abstract unsafe class App<TModel, TMsg, TScene> : MonoBehaviour
            where TModel : unmanaged where TMsg : unmanaged where TScene : unmanaged {
        protected abstract Runtime<TModel, TMsg, TScene>.Config Config { get; }
        protected abstract IPresenter<TScene> NewPresenter(Transform parent);

        private IPresenter<TScene> _presenter;

        protected Runtime<TModel, TMsg, TScene> Runtime { get; private set; }

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private bool _useDumpedModel;
        [SerializeField] [HideInInspector] private byte[] _model;
        private static TModel _dumpedModel;

        private static (TModel, L<Cmd<TMsg>>) InitModelFromDump() {
            return (_dumpedModel, new());
        }

        protected virtual void OnEnable() {
            var config = Config;
            _presenter = NewPresenter(gameObject.transform);
            if (_useDumpedModel) {
                config.Init.Dispose();
                config.Init = CLf.New(&InitModelFromDump);
            }
            Runtime = new Runtime<TModel, TMsg, TScene>(config, _presenter);

            if (_useDumpedModel) {
                fixed (byte* buf = _model) {
                    if (!Serializer.Deserialize<TModel>(buf).Success(out _dumpedModel, out var error)) {
                        Debug.Log(error);
                    }
                }
            }
            if (Config.Reset.Test(out var reset)) {
                reset.Invoke(_dumpedModel);
            }
            Runtime.Run();
        }

        protected void OnDisable() {
            _useDumpedModel = true;

            var sz = Serializer.GetSize(Runtime.Model);

            if ((_model == null) || (_model.Length < sz)) {
                _model = new byte[sz];
            }

            fixed (byte* buf = _model) {
                Serializer.Serialize(Runtime.Model, buf);
            }

            _presenter.Dispose();
        }
#else
        protected virtual void OnEnable() { }
        
        protected void OnDisable() { }

        protected virtual void Start() {
            Runtime = new Runtime<TModel, TMsg, TScene>(Config, NewPresenter(gameObject.transform));
            Runtime.Run();
        }
#endif

        protected virtual void Update() {
            Runtime.TriggerSub(new Timer.FrameData(Time.timeAsDouble, Time.deltaTime));
        }

        protected virtual void FixedUpdate() {
            Runtime.TriggerSub(new Timer.TickData(Time.timeAsDouble, Time.deltaTime));
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            Access.__DomainReload();
        }
#endif
    }
}