using System;
using Rondo.Core;
using Rondo.Core.Lib;
using UnityEngine;

namespace Rondo.Unity.Subs {
    public static class Timer {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void __DomainReload() {
            Subscribe();
        }
#endif

        static Timer() {
            Subscribe();
        }

        private static void Subscribe() {
            Presenter.OnFrame += (dt, p) => p.MessageReceiver.TriggerSub(new FrameData(Time.timeAsDouble, dt));
            Presenter.OnFixedUpdate += (dt, p) => p.MessageReceiver.TriggerSub(new TickData(Time.timeAsDouble, dt));
        }

        public static ISub Frame(Func<FrameData, IMsg> toMsg) {
            return new Sub<FrameData>(toMsg);
        }

        public readonly struct FrameData {
            public readonly double Time;
            public readonly float Delta;

            public FrameData(double time, float delta) {
                Time = time;
                Delta = delta;
            }
        }

        public static ISub Tick(Func<TickData, IMsg> toMsg) {
            return new Sub<TickData>(toMsg);
        }

        public readonly struct TickData {
            public readonly double Time;
            public readonly float Delta;

            public TickData(double time, float delta) {
                Time = time;
                Delta = delta;
            }
        }

        public static ICmd RequestTime(Func<double, IMsg> toMsg) {
            return new Cmd<double>(() => Time.timeAsDouble, toMsg);
        }

        public static ICmd RequestNow(Func<DateTime, IMsg> toMsg) {
            return new Cmd<DateTime>(() => DateTime.Now, toMsg);
        }
    }
}