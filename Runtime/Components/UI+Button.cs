using System;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct ButtonConfig<TMsg> {
            public readonly bool Interactable;
            public readonly Transition Transition;
            public readonly delegate*<TMsg> OnClick;
            public readonly ObjRef TargetGraphic;
            public readonly ColorBlock Colors;
            public readonly SpriteAddressState SpriteAddresses;

            public ButtonConfig(
                delegate*<TMsg> onClick,
                bool interactable = true,
                Transition transition = Transition.None,
                ColorBlock colors = default,
                SpriteAddressState spriteAddresses = default,
                ObjRef targetGraphic = default
            ) {
                Interactable = interactable;
                Transition = transition;
                Colors = colors;
                SpriteAddresses = spriteAddresses;
                OnClick = onClick;
                TargetGraphic = targetGraphic;
            }
        }

        private static readonly ulong _idButton = CompExtensions.NextId;

        public static Comp Button<TMsg>(ButtonConfig<TMsg> config) where TMsg : unmanaged {
            return new Comp(_idButton, &SyncButton<TMsg>, Mem.C.CopyPtr(config));
        }

        private static void SyncButton<TMsg>(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext)
                where TMsg : unmanaged {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<Button>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<Button>();
            }

            var button = gameObject.GetComponent<Button>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<ButtonConfig<TMsg>>();
            var next = *pNext.Cast<ButtonConfig<TMsg>>();

            if (force || (prev.Interactable != next.Interactable)) {
                button.interactable = next.Interactable;
            }
            if (force || (prev.Transition != next.Transition)) {
                button.transition = (Selectable.Transition)next.Transition;
            }
#pragma warning disable 8909
            if (force || (prev.OnClick != next.OnClick)) {
#pragma warning  restore 8909
                button.onClick.RemoveAllListeners();
                //TODO: try to remove allocation
                if (next.OnClick != null) {
                    button.onClick.AddListener(() => presenter.Messenger.PostMessage(next.OnClick()));
                }
            }
            if (next.TargetGraphic == ObjRef.NoRef) {
                if (prev.TargetGraphic != ObjRef.NoRef) {
                    button.targetGraphic = null;
                }
            }
            else {
                presenter.RequestObjRef(next.TargetGraphic, gameObject, Ca.New<GameObject, GameObject>(&HandleTargetGraphicsRef));
            }

            if (force || !prev.Colors.Equals(next.Colors)) {
                button.colors = next.Colors.UnityColorBlock;
            }
            if (force || !prev.SpriteAddresses.Equals(next.SpriteAddresses)) {
                if (prev.SpriteAddresses.Disabled != next.SpriteAddresses.Disabled) {
                    AddressablesCache.Load(
                        next.SpriteAddresses.Disabled, _spriteType, gameObject, HandleButtonSpriteLoaded(ButtonSpriteKind.Disabled)
                    );
                }
                if (prev.SpriteAddresses.Highlighted != next.SpriteAddresses.Highlighted) {
                    AddressablesCache.Load(
                        next.SpriteAddresses.Highlighted, _spriteType, gameObject, HandleButtonSpriteLoaded(ButtonSpriteKind.Highlighted)
                    );
                }
                if (prev.SpriteAddresses.Pressed != next.SpriteAddresses.Pressed) {
                    AddressablesCache.Load(
                        next.SpriteAddresses.Pressed, _spriteType, gameObject, HandleButtonSpriteLoaded(ButtonSpriteKind.Pressed)
                    );
                }
                if (prev.SpriteAddresses.Selected != next.SpriteAddresses.Selected) {
                    AddressablesCache.Load(
                        next.SpriteAddresses.Selected, _spriteType, gameObject, HandleButtonSpriteLoaded(ButtonSpriteKind.Selected)
                    );
                }
            }
        }

        private static void HandleTargetGraphicsRef(GameObject gameObject, GameObject target) {
            gameObject.GetComponent<Button>().targetGraphic = target.GetComponent<Graphic>();
        }

        private static CLa<GameObject, Object> HandleButtonSpriteLoaded(ButtonSpriteKind bsk) {
            static void Impl(GameObject gameObject, Object sprite, ButtonSpriteKind* bsk) {
                var button = gameObject.GetComponent<Button>();
                var spriteState = button.spriteState;
                switch (*bsk) {
                    case ButtonSpriteKind.Disabled:
                        spriteState.disabledSprite = (Sprite)sprite;
                        break;
                    case ButtonSpriteKind.Highlighted:
                        spriteState.highlightedSprite = (Sprite)sprite;
                        break;
                    case ButtonSpriteKind.Pressed:
                        spriteState.pressedSprite = (Sprite)sprite;
                        break;
                    case ButtonSpriteKind.Selected:
                        spriteState.selectedSprite = (Sprite)sprite;
                        break;
                }
                button.spriteState = spriteState;
            }

            return CLa.New<GameObject, Object, ButtonSpriteKind>(&Impl, bsk);
        }

        private enum ButtonSpriteKind {
            Disabled,
            Highlighted,
            Pressed,
            Selected
        }

        public enum Transition {
            None,
            ColorTint,
            SpriteSwap,
            Animation,
        }

        public readonly struct ColorBlock : IEquatable<ColorBlock> {
            public readonly float4 NormalColor;
            public readonly float4 HighlightedColor;
            public readonly float4 PressedColor;
            public readonly float4 SelectedColor;
            public readonly float4 DisabledColor;
            public readonly float ColorMultiplier;
            public readonly float FadeDuration;

            public ColorBlock(
                float4 normalColor,
                float4 highlightedColor,
                float4 pressedColor,
                float4 selectedColor,
                float4 disabledColor,
                float colorMultiplier = 1,
                float fadeDuration = 0.1f
            ) {
                NormalColor = normalColor;
                HighlightedColor = highlightedColor;
                PressedColor = pressedColor;
                SelectedColor = selectedColor;
                DisabledColor = disabledColor;
                ColorMultiplier = colorMultiplier;
                FadeDuration = fadeDuration;
            }

            public bool Equals(ColorBlock other) {
                return NormalColor.Equals(other.NormalColor)
                        && HighlightedColor.Equals(other.HighlightedColor)
                        && PressedColor.Equals(other.PressedColor)
                        && SelectedColor.Equals(other.SelectedColor)
                        && DisabledColor.Equals(other.DisabledColor)
                        && ColorMultiplier.Equals(other.ColorMultiplier)
                        && FadeDuration.Equals(other.FadeDuration);
            }

            internal UnityEngine.UI.ColorBlock UnityColorBlock => new() {
                    colorMultiplier = ColorMultiplier,
                    fadeDuration = FadeDuration,
                    disabledColor = Colors.FromFloat4(DisabledColor),
                    highlightedColor = Colors.FromFloat4(HighlightedColor),
                    normalColor = Colors.FromFloat4(NormalColor),
                    pressedColor = Colors.FromFloat4(PressedColor),
                    selectedColor = Colors.FromFloat4(SelectedColor),
            };
        }

        public readonly struct SpriteAddressState : IEquatable<SpriteAddressState> {
            public readonly S Highlighted;
            public readonly S Pressed;
            public readonly S Selected;
            public readonly S Disabled;

            public SpriteAddressState(S highlighted, S pressed, S selected, S disabled) {
                Highlighted = highlighted;
                Pressed = pressed;
                Selected = selected;
                Disabled = disabled;
            }

            public bool Equals(SpriteAddressState other) {
                return Highlighted.Equals(other.Highlighted)
                        && Pressed.Equals(other.Pressed)
                        && Selected.Equals(other.Selected)
                        && Disabled.Equals(other.Disabled);
            }
        }
    }
}