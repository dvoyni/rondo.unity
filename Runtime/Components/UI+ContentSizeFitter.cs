using Rondo.Core.Memory;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct ContentSizeFitterConfig {
            public readonly FitMode HorizontalFit;
            public readonly FitMode VerticalFit;

            public ContentSizeFitterConfig(
                FitMode horizontalFit = FitMode.Unconstrained,
                FitMode verticalFit = FitMode.Unconstrained
            ) {
                HorizontalFit = horizontalFit;
                VerticalFit = verticalFit;
            }
        }

        private static readonly ulong _idContentSizeFitter = CompExtensions.NextId;

        public static Comp ContentSizeFitter(ContentSizeFitterConfig config) {
            return new Comp(_idContentSizeFitter, &SyncContentSizeFitter, Mem.C.CopyPtr(config));
        }

        private static void SyncContentSizeFitter(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<ContentSizeFitter>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<ContentSizeFitter>();
            }

            var fitter = gameObject.GetComponent<ContentSizeFitter>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<ContentSizeFitterConfig>();
            var next = *pNext.Cast<ContentSizeFitterConfig>();

            if (force || (prev.HorizontalFit != next.HorizontalFit)) {
                fitter.horizontalFit = (ContentSizeFitter.FitMode)next.HorizontalFit;
            }
            if (force || (prev.VerticalFit != next.VerticalFit)) {
                fitter.verticalFit = (ContentSizeFitter.FitMode)next.VerticalFit;
            }
        }

        public enum FitMode {
            Unconstrained,
            MinSize,
            PreferredSize,
        }
    }
}