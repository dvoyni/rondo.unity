using System;
using Rondo.Core;
using Rondo.Unity.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Rondo.Unity.Components {
    public static partial class UI {
        public readonly struct Button : IComp {
            public readonly bool Interactable;
            public readonly Transition Transition;
            public readonly IMsg OnClick;
            public readonly ObjRef TargetGraphic;
            public readonly ColorBlock Colors;
            public readonly SpriteAddressState SpriteAddresses;

            public Button(
                IMsg onClick,
                bool interactable = true,
                Transition transition = Transition.None,
                ColorBlock colors = default,
                SpriteAddressState spriteAddresses = default,
                ObjRef targetGraphic = default
            ) {
                Transition = transition;
                Colors = colors;
                SpriteAddresses = spriteAddresses;
                Interactable = interactable;
                OnClick = onClick;
                TargetGraphic = targetGraphic;
            }

            public void Sync(IPresenter presenter, GameObject gameObject, IComp cPrev) {
                var create = cPrev == null;

                var button = create ? gameObject.AddComponent<UnityEngine.UI.Button>() : gameObject.GetComponent<UnityEngine.UI.Button>();
                var prev = create ? default : (Button)cPrev;

                if (create || (prev.Interactable != Interactable)) {
                    button.interactable = Interactable;
                }
                if (create || (prev.Transition != Transition)) {
                    button.transition = (Selectable.Transition)Transition;
                }
#pragma warning disable 8909
                if (create || (prev.OnClick != OnClick)) {
#pragma warning  restore 8909
                    button.onClick.RemoveAllListeners();
                    var onClick = OnClick;
                    button.onClick.AddListener(() => presenter.MessageReceiver.PostMessage(onClick));
                }
                if (TargetGraphic == ObjRef.NoRef) {
                    if (prev.TargetGraphic != ObjRef.NoRef) {
                        button.targetGraphic = null;
                    }
                }
                else {
                    presenter.RequestObjRef(TargetGraphic, go => button.targetGraphic = go.GetComponent<Graphic>());
                }

                if (create || !prev.Colors.Equals(Colors)) {
                    button.colors = Colors.UnityColorBlock;
                }
                if (create || !prev.SpriteAddresses.Equals(SpriteAddresses)) {
                    if (prev.SpriteAddresses.Disabled != SpriteAddresses.Disabled) {
                        AddressablesCache.Load<Sprite>(
                            SpriteAddresses.Disabled, HandleButtonSpriteLoaded(ButtonSpriteKind.Disabled, button)
                        );
                    }
                    if (prev.SpriteAddresses.Highlighted != SpriteAddresses.Highlighted) {
                        AddressablesCache.Load<Sprite>(
                            SpriteAddresses.Highlighted, HandleButtonSpriteLoaded(ButtonSpriteKind.Highlighted, button)
                        );
                    }
                    if (prev.SpriteAddresses.Pressed != SpriteAddresses.Pressed) {
                        AddressablesCache.Load<Sprite>(
                            SpriteAddresses.Pressed, HandleButtonSpriteLoaded(ButtonSpriteKind.Pressed, button)
                        );
                    }
                    if (prev.SpriteAddresses.Selected != SpriteAddresses.Selected) {
                        AddressablesCache.Load<Sprite>(
                            SpriteAddresses.Selected, HandleButtonSpriteLoaded(ButtonSpriteKind.Selected, button)
                        );
                    }
                }
            }

            public void Remove(IPresenter presenter, GameObject gameObject) {
                Helpers.DestroySafe<UnityEngine.UI.Button>(gameObject);
            }
        }

        private static Action<Object> HandleButtonSpriteLoaded(ButtonSpriteKind bsk, UnityEngine.UI.Button button) =>
                sprite => {
                    var spriteState = button.spriteState;
                    switch (bsk) {
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
                };

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
            public readonly string Highlighted;
            public readonly string Pressed;
            public readonly string Selected;
            public readonly string Disabled;

            public SpriteAddressState(string highlighted, string pressed, string selected, string disabled) {
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