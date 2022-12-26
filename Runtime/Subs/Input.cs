using System;
using System.Collections.Generic;
using Rondo.Core;
using Rondo.Core.Lib;
using Unity.Mathematics;

namespace Rondo.Unity.Subs {
    public static class Input {
        public readonly struct PointerEventData {
            public readonly PointerEventKind Kind;
            public readonly PointerEventButton Button;
            public readonly float2 ScreenPosition;
            public readonly IEnumerable<object> ObjKeys;

            public PointerEventData(
                PointerEventKind kind, PointerEventButton button, float2 screenPosition, IEnumerable<object> objKeys
            ) {
                Kind = kind;
                Button = button;
                ScreenPosition = screenPosition;
                ObjKeys = objKeys;
            }
        }

        public enum PointerEventKind {
            Down,
            BeginDrag,
            Drag,
            EndDrag,
            Up,
            Click,
            Move
        }

        [Flags]
        public enum PointerEventButton {
            None = 0x00,
            Left = 0x01,
            Right = 0x02,
            Middle = 0x04,
        }

        public readonly struct CursorMoveData {
            public readonly float2 ScreenPosition;
            public readonly IEnumerable<object> ObjKeys;

            public CursorMoveData(float2 screenPosition, IEnumerable<object> objKeys) {
                ScreenPosition = screenPosition;
                ObjKeys = objKeys;
            }
        }

        public static ISub PointerEvent(Func<PointerEventData, IMsg> toMsg) {
            return new Sub<PointerEventData>(toMsg);
        }

        public static ISub CursorMove(Func<PointerEventData, IMsg> toMsg) {
            return new Sub<PointerEventData>(toMsg);
        }
    }
}