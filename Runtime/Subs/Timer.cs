using Rondo.Core;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using UnityEngine;

namespace Rondo.Unity.Subs {
    public static unsafe class Timer {
        public static Sub Frame<TMsg>(delegate*<FrameData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }

        public readonly struct FrameData {
            public readonly double Time;
            public readonly float Delta;

            public FrameData(double time, float delta) {
                Time = time;
                Delta = delta;
            }
        }

        public static Sub Tick<TMsg>(delegate*<TickData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }

        public readonly struct TickData {
            public readonly double Time;
            public readonly float Delta;

            public TickData(double time, float delta) {
                Time = time;
                Delta = delta;
            }
        }

        public static Cmd RequestTime<TMsg>(Xf<double, TMsg> toMsg)
                where TMsg : unmanaged {
            static void Impl(Ptr pPayload, Xf<Ptr, Ptr> toMsg, PostMessage post) {
                post.Invoke(toMsg, pPayload);
                toMsg.Dispose();
            }

            return Cmd.New(&Impl, toMsg, Time.timeAsDouble);
        }
    }
}