using System.IO;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;

namespace Rondo.Unity.Utils {
    public static unsafe class Debug {
        public static void Log(string message) {
            UnityEngine.Debug.Log(message);
        }

        public static T Log<T>(string message, T obj) where T : unmanaged {
            UnityEngine.Debug.Log($"{message}\n{Serializer.Stringify(obj)}");
            return obj;
        }

        public static T Log<T>(T obj) where T : unmanaged {
            UnityEngine.Debug.Log(Serializer.Stringify(obj));
            return obj;
        }

        public static TA Log<TA, TB>(TA a, TB b) where TA : unmanaged where TB : unmanaged {
            UnityEngine.Debug.Log($"{Serializer.Stringify(a)}\n{Serializer.Stringify(b)}");
            return a;
        }

        public static Xa<TModel, TMsg, TModel, A<Cmd>> DumpUpdate<TModel, TMsg>(
            S path = default,
            Maybe<Xf<TModel, TMsg, Maybe<S>>> getName = default
        ) where TMsg : unmanaged where TModel : unmanaged {
            static void Impl(
                TModel model,
                TMsg msg,
                TModel nextModel,
                A<Cmd> cmd,
                S* path,
                Xf<TModel, TMsg, Maybe<S>>* getName
            ) {
                var dir = (string)*path;
                Directory.CreateDirectory(dir);
                if (!getName->Invoke(model, msg).Test(out var x)) {
                    return;
                }
                var name = Path.Join(dir, (string)x);
                {
                    var sz = Serializer.Serialize(model, null);
                    var buf = new byte[sz];
                    fixed (byte* p = buf) {
                        Serializer.Serialize(model, p);
                    }
                    File.WriteAllBytes($"{name}.model.bin", buf);
                }
                {
                    var sz = Serializer.Serialize(msg, null);
                    var buf = new byte[sz];
                    fixed (byte* p = buf) {
                        Serializer.Serialize(msg, p);
                    }
                    File.WriteAllBytes($"{name}.msg.bin", buf);
                }
                {
                    var sz = Serializer.Serialize(nextModel, null);
                    var buf = new byte[sz];
                    fixed (byte* p = buf) {
                        Serializer.Serialize(nextModel, p);
                    }
                    File.WriteAllBytes($"{name}.next-model.bin", buf);
                }
                {
                    File.WriteAllText($"{name}.model.txt", Serializer.Stringify(model));
                }
                {
                    File.WriteAllText($"{name}.msg.txt", Serializer.Stringify(msg));
                }
                {
                    File.WriteAllText($"{name}.next-model.txt", Serializer.Stringify(nextModel));
                }
            }

            static Maybe<S> DefaultName(TModel model, TMsg msg) {
                return Maybe<S>.Just((S)"last");
            }

            if (path == S.Empty) {
                path = (S)DebugDumDir;
            }
            if (!getName.Test(out var nfn)) {
                nfn = Xf.New<TModel, TMsg, Maybe<S>>(&DefaultName);
            }
            return Xa.New<TModel, TMsg, TModel, A<Cmd>, S, Xf<TModel, TMsg, Maybe<S>>>(&Impl, path, nfn);
        }

        public const string DebugDumDir = "DebugDump";
    }
}