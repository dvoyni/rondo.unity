using Rondo.Core.Memory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.Unity.Components {
    public static unsafe partial class UI {
        public readonly struct GridLayoutGroupConfig {
            public readonly int4 Padding;
            public readonly float2 CellSize;
            public readonly float2 Spacing;
            public readonly TextAnchor ChildAlignment;
            public readonly Corner StartCorner;
            public readonly Axis StartAxis;
            public readonly Constraint Constraint;
            public readonly int ConstraintCount;

            public GridLayoutGroupConfig(
                float2 cellSize,
                int constraintCount = 2,
                int4 padding = default,
                float2 spacing = default,
                TextAnchor childAlignment = TextAnchor.UpperLeft,
                Corner startCorner = Corner.UpperLeft,
                Axis startAxis = Axis.Horizontal,
                Constraint constraint = Constraint.Flexible
            ) {
                Padding = padding;
                CellSize = cellSize;
                ConstraintCount = constraintCount;
                Spacing = spacing;
                ChildAlignment = childAlignment;
                StartCorner = startCorner;
                StartAxis = startAxis;
                Constraint = constraint;
            }
        }

        private static readonly ulong _idGridLayoutGroup = CompExtensions.NextId;

        public static Comp GridLayoutGroup(GridLayoutGroupConfig config) {
            return new Comp(_idGridLayoutGroup, &SyncGridLayoutGroup, Mem.C.CopyPtr(config));
        }

        private static void SyncGridLayoutGroup(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pPrev == pNext) {
                return;
            }
            if (pNext == Ptr.Null) {
                Utils.Utils.DestroySafe<GridLayoutGroup>(gameObject);
                return;
            }
            if (pPrev == Ptr.Null) {
                gameObject.AddComponent<GridLayoutGroup>();
            }

            var group = gameObject.GetComponent<GridLayoutGroup>();
            var force = pPrev == Ptr.Null;
            var prev = force ? default : *pPrev.Cast<GridLayoutGroupConfig>();
            var next = *pNext.Cast<GridLayoutGroupConfig>();

            if (force || !prev.Padding.Equals(next.Padding)) {
                group.padding = new RectOffset(next.Padding.w, next.Padding.y, next.Padding.x, next.Padding.z);
            }
            if (force || !prev.CellSize.Equals(next.CellSize)) {
                group.cellSize = next.CellSize;
            }
            if (force || !prev.Spacing.Equals(next.Spacing)) {
                group.spacing = next.Spacing;
            }
            if (force || (prev.ChildAlignment != next.ChildAlignment)) {
                group.childAlignment = (UnityEngine.TextAnchor)next.ChildAlignment;
            }
            if (force || (prev.StartCorner != next.StartCorner)) {
                group.startCorner = (GridLayoutGroup.Corner)next.StartCorner;
            }
            if (force || (prev.StartAxis != next.StartAxis)) {
                group.startAxis = (GridLayoutGroup.Axis)next.StartAxis;
            }
            if (force || (prev.Constraint != next.Constraint)) {
                group.constraint = (GridLayoutGroup.Constraint)next.Constraint;
            }
            if (force || (prev.ConstraintCount != next.ConstraintCount)) {
                group.constraintCount = next.ConstraintCount;
            }
        }

        public enum Corner {
            UpperLeft = 0,
            UpperRight = 1,
            LowerLeft = 2,
            LowerRight = 3,
        }

        public enum Axis {
            Horizontal = 0,
            Vertical = 1,
        }

        public enum Constraint {
            Flexible = 0,
            FixedColumnCount = 1,
            FixedRowCount = 2,
        }
    }
}