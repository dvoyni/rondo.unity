using Rondo.Core;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using Rondo.Unity.Subs;
using UnityEngine;

namespace Rondo.Unity {
    public abstract unsafe class App<TModel, TMsg, TScene> : MonoBehaviour
            where TModel : unmanaged where TMsg : unmanaged where TScene : unmanaged {
        protected abstract Runtime<TModel, TMsg, TScene>.Config NewConfig { get; }
        protected abstract IPresenter<TScene> NewPresenter(Transform parent);

        private Runtime<TModel, TMsg, TScene>.Config _config;
        private IPresenter<TScene> _presenter;

        protected Runtime<TModel, TMsg, TScene> Runtime { get; private set; }

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private bool _useDumpedModel;
        [SerializeField] [HideInInspector] private byte[] _model;
        private static TModel _dumpedModel;

        private static (TModel, A<Cmd>) InitModelFromDump() {
            return (_dumpedModel, new());
        }

        protected virtual void OnEnable() {
            Prepare();
            PrepareEditor();
            Run();
        }

        protected void OnDisable() {
            CleanupEditor();
            Cleanup();
        }

        private void PrepareEditor() {
            if (_useDumpedModel) {
                _config.Init.Dispose();
                _config.Init = Xf.New(&InitModelFromDump);
            }

            CreateRuntime();

            if (_useDumpedModel) {
                fixed (byte* buf = _model) {
                    if (!Serializer.Deserialize<TModel>(buf).Success(out _dumpedModel, out var error)) {
                        Debug.Log(error);
                    }
                }
            }
            if (_config.Reset.Test(out var reset)) {
                reset.Invoke(_dumpedModel);
            }
        }

        private void CleanupEditor() {
            _useDumpedModel = true;

            var sz = Serializer.GetSize(Runtime.Model);

            if ((_model == null) || (_model.Length < sz)) {
                _model = new byte[sz];
            }

            fixed (byte* buf = _model) {
                Serializer.Serialize(Runtime.Model, buf);
            }
        }
#else
        protected virtual void OnEnable() { }
        
        protected void OnDisable() { }

        protected virtual void Start() {
            Prepare();
            CreateRuntime();
            Run();
        }
        
        protected virtual void OnDestroy() {
            Cleanup();
        }
#endif

        private void Prepare() {
            Mem.Manager = new MemManager();
            _config = NewConfig;
            _presenter = NewPresenter(gameObject.transform);
        }

        private void CreateRuntime() {
            Runtime = new Runtime<TModel, TMsg, TScene>(_config, _presenter);
        }

        private void Run() {
            Runtime.Run();
        }

        private void Cleanup() {
            _config.Dispose();
            _presenter.Dispose();
        }

        protected virtual void Update() {
            Runtime.TriggerSub(new Timer.FrameData(Time.timeAsDouble, Time.deltaTime));
        }

        protected virtual void FixedUpdate() {
            Runtime.TriggerSub(new Timer.TickData(Time.timeAsDouble, Time.deltaTime));
        }
    }
}