using System.Collections.Generic;
using Rondo.Core;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using Unity.Mathematics;

namespace Rondo.Unity.Cmds {
    public static unsafe class Tweener {
        private static readonly Dictionary<ulong, Tw> _tweeners = new();
        private static readonly List<ulong> _tmpIds = new();
        private static ulong _lastId;
        private static double _time;

        static Tweener() {
            Lifecycle.OnUpdate += CheckTweeners;
        }

        public readonly struct State {
            public readonly float Value;
            public readonly float Progress;
            public readonly ulong Id;

            public State(float value, float progress, ulong id) {
                Value = value;
                Progress = progress;
                Id = id;
            }
        }

        private static Cmd<TMsg> Start<TMsg>(float length, delegate*<float, float> ease, delegate*<State, TMsg> toMsg)
                where TMsg : unmanaged {
            //TODO: cannot be stored. should recreate every mem swap
            static void Impl(Ptr pPayload, L<Cf<Ptr, Ptr>> toMsg, PostMessage post) {
                var id = ++_lastId;
                _tweeners[id] = new Tw {
                        Args = *pPayload.Cast<TwArgs>(),
                        Post = post,
                        ToMsg = toMsg
                };
            }

            return Cmd.New(&Impl, Cf.New(toMsg), new TwArgs { Start = _time, Length = length, Ease = ease });
        }

        public static void Stop(ulong id) {
            _tweeners.Remove(id);
        }

        private static void CheckTweeners(float deltaTime, IPresenter presenter) {
            _time += deltaTime;
            _tmpIds.Clear();

            foreach (var (id, tw) in _tweeners) {
                var t = math.saturate((float)(_time - tw.Args.Start) / tw.Args.Length);
                var u = tw.Args.Ease(t);
                tw.Post(tw.ToMsg, Mem.C.CopyPtr(new State(u, t, id)));
                if (t >= 1) {
                    _tmpIds.Add(id);
                }
            }

            foreach (var id in _tmpIds) {
                _tweeners.Remove(id);
            }
        }

        private struct TwArgs {
            public double Start;
            public float Length;
            public delegate*<float, float> Ease;
        }

        private struct Tw {
            public TwArgs Args;
            public PostMessage Post;
            public L<Cf<Ptr, Ptr>> ToMsg;
        }

        public static float EaseLinear(float x) => x;
        public static float EaseInQuad(float x) => x * x;
        public static float EaseOutQuad(float x) => 1 - (1 - x) * (1 - x);
        public static float EaseInOutQuad(float x) => x < 0.5 ? 2 * x * x : 1 - (-2 * x + 2) * (-2 * x + 2) / 2;
    }
}