using System.Diagnostics;
using System.Runtime.InteropServices;
using Rondo.Core.Extras;
using Rondo.Core.Lib.Containers;

namespace Rondo.Unity {
    //TODO: tags support, layers support
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Obj {
        internal static ulong _lastIndex;

        public S Name;
        public D<ulong, Comp> Components;
        public D<S, Obj> Children;
        public bool Inactive;
        public bool Static;
        public Key Key;
        public ObjRef Ref;

        public Obj(Obj other) {
            this = other;
        }

        public Obj(
            string name,
            L<Comp> components = default,
            L<Obj> children = default,
            bool inactive = default,
            bool @static = default,
            Key key = default,
            ObjRef objRef = default
        ) {
            CheckChildrenNames(children);

            Name = (S)(string.IsNullOrEmpty(name) ? $"Obj{++_lastIndex}" : name);
            Inactive = inactive;
            Static = @static;
            Components = components.ToDMap(&ObjExtensions.ToObjComp);
            Children = children.ToDMap(&ObjExtensions.ToObjChild);
            Key = key;
            Ref = objRef;
        }

        [Conditional("DEBUG")]
        private static void CheckChildrenNames(L<Obj> children) {
            static S GetName(Obj obj) => obj.Name;

            var sorted = children.SortBy(&GetName);
            for (var i = 1; i < sorted.Length(); i++) {
                if (sorted.At(i - 1).Test(out var a) && sorted.At(i).Test(out var b)) {
                    Assert.That(a.Name != b.Name, $"Obj children should have unique names ({(string)a.Name})");
                }
            }
        }
    }
}