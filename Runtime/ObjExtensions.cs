using Rondo.Core.Lib.Containers;

namespace Rondo.Unity {
    public static class ObjExtensions {
        public static P<ulong, Comp> ToObjComp(this Comp c) {
            return new(c.Id, c);
        }

        public static P<S, Obj> ToObjChild(this Obj obj) {
            return new(obj.Name, obj);
        }
    }
}