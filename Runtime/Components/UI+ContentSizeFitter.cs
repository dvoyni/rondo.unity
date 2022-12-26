using Rondo.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct ContentSizeFitter : IComp {
            public readonly FitMode HorizontalFit;
            public readonly FitMode VerticalFit;

            public ContentSizeFitter(
                FitMode horizontalFit = FitMode.Unconstrained,
                FitMode verticalFit = FitMode.Unconstrained
            ) {
                HorizontalFit = horizontalFit;
                VerticalFit = verticalFit;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var fitter = create
                        ? gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>()
                        : gameObject.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                var prev = create ? default : (ContentSizeFitter)cPrev;

                if (create || (prev.HorizontalFit != HorizontalFit)) {
                    fitter.horizontalFit = (UnityEngine.UI.ContentSizeFitter.FitMode)HorizontalFit;
                }
                if (create || (prev.VerticalFit != VerticalFit)) {
                    fitter.verticalFit = (UnityEngine.UI.ContentSizeFitter.FitMode)VerticalFit;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.ContentSizeFitter>(gameObject);
            }
        }

        public enum FitMode {
            Unconstrained,
            MinSize,
            PreferredSize,
        }
    }
}