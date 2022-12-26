using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct Text : IComp {
            public readonly string Value;
            public readonly string FontAddress;
            public readonly FontStyle FontStyle;
            public readonly int FontSize;
            public readonly float LineSpacing;
            public readonly bool RichText;
            public readonly TextAnchor Align;
            public readonly bool AlignByGeometry;
            public readonly HorizontalWrapMode HorizontalWrapMode;
            public readonly VerticalWrapMode VerticalWrapMode;
            public readonly bool BestFit;
            public readonly int2 BestFitFontSize;
            public readonly float4 Color;
            public readonly string MaterialAddress;
            public readonly bool RaycastTarget;
            public readonly float4 RaycastPadding;
            public readonly bool Maskable;

            public Text(
                string value,
                float4 color,
                string fontAddress = default,
                FontStyle fontStyle = FontStyle.Normal,
                int fontSize = 14,
                float lineSpacing = 1,
                bool richText = true,
                TextAnchor align = TextAnchor.UpperLeft,
                bool alignByGeometry = false,
                HorizontalWrapMode horizontalWrapMode = HorizontalWrapMode.Wrap,
                VerticalWrapMode verticalWrapMode = VerticalWrapMode.Truncate,
                bool bestFit = false,
                int2 bestFitFontSize = default,
                string materialAddress = default,
                bool raycastTarget = true,
                float4 raycastPadding = default,
                bool maskable = true
            ) {
                Value = value;
                FontAddress = fontAddress;
                FontStyle = fontStyle;
                FontSize = fontSize;
                LineSpacing = lineSpacing;
                RichText = richText;
                Align = align;
                AlignByGeometry = alignByGeometry;
                HorizontalWrapMode = horizontalWrapMode;
                VerticalWrapMode = verticalWrapMode;
                BestFit = bestFit;
                BestFitFontSize = bestFitFontSize;
                Color = color;
                MaterialAddress = materialAddress;
                RaycastTarget = raycastTarget;
                RaycastPadding = raycastPadding;
                Maskable = maskable;
            }

            public Text(
                string value,
                string fontAddress = default,
                FontStyle fontStyle = FontStyle.Normal,
                int fontSize = 14,
                float lineSpacing = 1,
                bool richText = true,
                TextAnchor align = TextAnchor.UpperLeft,
                bool alignByGeometry = false,
                HorizontalWrapMode horizontalWrapMode = HorizontalWrapMode.Wrap,
                VerticalWrapMode verticalWrapMode = VerticalWrapMode.Truncate,
                bool bestFit = false,
                int2 bestFitFontSize = default,
                string materialAddress = default,
                bool raycastTarget = true,
                float4 raycastPadding = default,
                bool maskable = true
            ) : this(
                value, 1, fontAddress, fontStyle, fontSize, lineSpacing, richText, align, alignByGeometry,
                horizontalWrapMode, verticalWrapMode, bestFit, bestFitFontSize, materialAddress,
                raycastTarget, raycastPadding, maskable
            ) { }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;
                var text = create ? gameObject.AddComponent<UnityEngine.UI.Text>() : gameObject.GetComponent<UnityEngine.UI.Text>();
                var prev = create ? default : (Text)cPrev;

                if (create || (prev.Value != Value)) {
                    text.text = Value;
                }
                if (create || (prev.FontAddress != FontAddress)) {
                    if (string.IsNullOrEmpty(FontAddress)) {
                        text.font = null;
                    }
                    else {
                        AddressablesCache.Load<Font>(FontAddress, font => text.font = (Font)font);
                    }
                }
                if (create || (prev.FontStyle != FontStyle)) {
                    text.fontStyle = (UnityEngine.FontStyle)FontStyle;
                }
                if (create || (prev.FontSize != FontSize)) {
                    text.fontSize = FontSize;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (create || (prev.LineSpacing != LineSpacing)) {
                    text.lineSpacing = LineSpacing;
                }
                if (create || (prev.RichText != RichText)) {
                    text.supportRichText = RichText;
                }
                if (create || (prev.Align != Align)) {
                    text.alignment = (UnityEngine.TextAnchor)Align;
                }
                if (create || !prev.AlignByGeometry.Equals(AlignByGeometry)) {
                    text.alignByGeometry = AlignByGeometry;
                }
                if (create || (prev.HorizontalWrapMode != HorizontalWrapMode)) {
                    text.horizontalOverflow = (UnityEngine.HorizontalWrapMode)HorizontalWrapMode;
                }
                if (create || (prev.VerticalWrapMode != VerticalWrapMode)) {
                    text.verticalOverflow = (UnityEngine.VerticalWrapMode)VerticalWrapMode;
                }
                if (create || (prev.BestFit != BestFit)) {
                    text.resizeTextForBestFit = BestFit;
                }
                if (create || !prev.BestFitFontSize.Equals(BestFitFontSize)) {
                    text.resizeTextMinSize = BestFitFontSize.x;
                    text.resizeTextMaxSize = BestFitFontSize.y;
                }
                if (create || !prev.Color.Equals(Color)) {
                    text.color = Colors.FromFloat4(Color);
                }
                if (create || (prev.MaterialAddress != MaterialAddress)) {
                    if (string.IsNullOrEmpty(MaterialAddress)) {
                        text.material = null;
                    }
                    else {
                        AddressablesCache.Load<Material>(MaterialAddress, mat => text.material = (Material)mat);
                    }
                }
                if (create || (prev.RaycastTarget != RaycastTarget)) {
                    text.raycastTarget = RaycastTarget;
                }
                if (create || !prev.RaycastPadding.Equals(RaycastPadding)) {
                    text.raycastPadding = RaycastPadding;
                }
                if (create || (prev.Maskable != Maskable)) {
                    text.maskable = Maskable;
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.Text>(gameObject);
            }
        }

        private static void HandleTextFontLoaded(GameObject gameObject, Object font) {
            gameObject.GetComponent<UnityEngine.UI.Text>().font = (Font)font;
        }

        private static void HandleTextMaterialLoaded(GameObject gameObject, Object material) {
            gameObject.GetComponent<UnityEngine.UI.Text>().material = (Material)material;
        }

        public enum FontStyle {
            Normal,
            Bold,
            Italic,
            BoldAndItalic,
        }

        public enum HorizontalWrapMode {
            Wrap,
            Overflow,
        }

        public enum VerticalWrapMode {
            Truncate,
            Overflow,
        }
    }
}