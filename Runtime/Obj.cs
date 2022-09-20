using System.Runtime.InteropServices;
using Rondo.Core.Lib.Containers;

namespace Rondo.Unity {
    //TODO: tags support, layers support
    //TODO: check name duplicates in children
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
            Name = (S)(string.IsNullOrEmpty(name) ? $"Obj{++_lastIndex}" : name);
            Inactive = inactive;
            Static = @static;
            Components = components.ToDMap(&ObjExtensions.ToObjComp);
            Children = children.ToDMap(&ObjExtensions.ToObjChild);
            Key = key;
            Ref = objRef;
        }
    }
}