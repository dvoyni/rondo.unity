using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        private static readonly Ts _fontType = Ts.OfUnmanaged(typeof(Font));

        public readonly struct TextConfig {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly S Text;
            public readonly S FontAddress;
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
            public readonly S MaterialAddress;
            public readonly bool RaycastTarget;
            public readonly float4 RaycastPadding;
            public readonly bool Maskable;

            public TextConfig(
                float4 color,
                S text = default,
                S fontAddress = default,
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
                S materialAddress = default,
                bool raycastTarget = true,
                float4 raycastPadding = default,
                bool maskable = true
            ) {
                Text = text;
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
        }

        private static readonly ulong _idText = CompExtensions.NextId;

        public static Comp Text(TextConfig config) {
            return new Comp(_idText, &SyncText, Mem.C.CopyPtr(config));
        }

        private static void SyncText(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<Text>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<Text>();
            }

            var text = gameObject.GetComponent<Text>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<TextConfig>();
            var next = *pNext.Cast<TextConfig>();

            if (force || (prev.Text != next.Text)) {
                text.text = (string)next.Text;
            }
            if (force || (prev.FontAddress != next.FontAddress)) {
                if (next.FontAddress == S.Empty) {
                    text.font = null;
                }
                else {
                    AddressablesCache.Load(
                        next.FontAddress,
                        _fontType,
                        gameObject,
                        Xa.New<GameObject, Object>(&HandleTextFontLoaded)
                    );
                }
            }
            if (force || (prev.FontStyle != next.FontStyle)) {
                text.fontStyle = (UnityEngine.FontStyle)next.FontStyle;
            }
            if (force || (prev.FontSize != next.FontSize)) {
                text.fontSize = next.FontSize;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (force || (prev.LineSpacing != next.LineSpacing)) {
                text.lineSpacing = next.LineSpacing;
            }
            if (force || (prev.RichText != next.RichText)) {
                text.supportRichText = next.RichText;
            }
            if (force || (prev.Align != next.Align)) {
                text.alignment = (UnityEngine.TextAnchor)next.Align;
            }
            if (force || !prev.AlignByGeometry.Equals(next.AlignByGeometry)) {
                text.alignByGeometry = next.AlignByGeometry;
            }
            if (force || (prev.HorizontalWrapMode != next.HorizontalWrapMode)) {
                text.horizontalOverflow = (UnityEngine.HorizontalWrapMode)next.HorizontalWrapMode;
            }
            if (force || (prev.VerticalWrapMode != next.VerticalWrapMode)) {
                text.verticalOverflow = (UnityEngine.VerticalWrapMode)next.VerticalWrapMode;
            }
            if (force || (prev.BestFit != next.BestFit)) {
                text.resizeTextForBestFit = next.BestFit;
            }
            if (force || !prev.BestFitFontSize.Equals(next.BestFitFontSize)) {
                text.resizeTextMinSize = next.BestFitFontSize.x;
                text.resizeTextMaxSize = next.BestFitFontSize.y;
            }
            if (force || !prev.Color.Equals(next.Color)) {
                text.color = Colors.FromFloat4(next.Color);
            }
            if (force || (prev.MaterialAddress != next.MaterialAddress)) {
                if (next.MaterialAddress == S.Empty) {
                    text.material = null;
                }
                else {
                    AddressablesCache.Load(
                        next.MaterialAddress,
                        _materialType,
                        gameObject,
                        Xa.New<GameObject, Object>(&HandleTextMaterialLoaded)
                    );
                }
            }
            if (force || (prev.RaycastTarget != next.RaycastTarget)) {
                text.raycastTarget = next.RaycastTarget;
            }
            if (force || !prev.RaycastPadding.Equals(next.RaycastPadding)) {
                text.raycastPadding = next.RaycastPadding;
            }
            if (force || (prev.Maskable != next.Maskable)) {
                text.maskable = next.Maskable;
            }
        }

        private static void HandleTextFontLoaded(GameObject gameObject, Object font) {
            gameObject.GetComponent<Text>().font = (Font)font;
        }

        private static void HandleTextMaterialLoaded(GameObject gameObject, Object material) {
            gameObject.GetComponent<Text>().material = (Material)material;
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