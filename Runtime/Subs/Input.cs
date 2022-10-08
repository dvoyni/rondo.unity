using System;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Lib.Platform;
using Rondo.Core.Memory;
using Unity.Mathematics;

namespace Rondo.Unity.Subs {
    public static unsafe class Input {
        public readonly struct PointerEventData {
            public readonly PointerEventKind Kind;
            public readonly PointerEventButton Button;
            public readonly float2 ScreenPosition;
            public readonly L<Key> ObjKeys;

            public PointerEventData(PointerEventKind kind, PointerEventButton button, float2 screenPosition, L<Key> objKeys) {
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
            public readonly L<Ptr> ObjKeys;

            public CursorMoveData(float2 screenPosition, L<Ptr> objKeys) {
                ScreenPosition = screenPosition;
                ObjKeys = objKeys;
            }
        }

        public static Sub PointerEvent<TMsg>(delegate*<PointerEventData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }

        public static Sub CursorMove<TMsg>(delegate*<PointerEventData, Maybe<TMsg>> toMsg)
                where TMsg : unmanaged {
            return Sub.New(toMsg);
        }
    }
}