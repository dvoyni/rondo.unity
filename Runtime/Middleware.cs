using Rondo.Core;
using Rondo.Unity.Managed;
using UnityEngine;

namespace Rondo.Unity {
    public static class Middleware<TScene> {
#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        private static IModel _dumpedModel;

        public static Runtime<TScene>.Config WithHotReload(Runtime<TScene>.Config config) {
            return new Runtime<TScene>.Config {
                    Init = flags => {
                        if (HotReloadComponent.Instance.UseDumpedModel) {
                            return (_dumpedModel, null);
                        }
                        return config.Init(flags);
                    },
                    Update = (message, model) => {
                        var p = config.Update(message, model);
                        _dumpedModel = p.Item1;
                        HotReloadComponent.Instance.UseDumpedModel = true;
                        return p;
                    },
                    Subscribe = config.Subscribe,
                    View = config.View,
            };
        } 
#else
        public static Runtime<TFlags, TModel, TScene>.Config WithHotReload(
            Runtime<TFlags, TModel, TScene>.Config config
        ) {
            return config;
        }
#endif
    }
}