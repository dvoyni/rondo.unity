using System;
using System.Linq;
using Rondo.Core.Lib;

namespace Rondo.Unity {
    //TODO: tags support, layers support
    public struct Obj {
        // ReSharper disable once StaticMemberInGenericType
        internal static ulong LastIndex;

        public string Name;
        public Dict<Type, IComp> Components;
        public Dict<string, Obj> Children;
        public bool Inactive;
        public bool Static;
        public object Key;
        public ObjRef Ref;

        public Obj(
            string name,
            Arr<IComp> components = default,
            Arr<Obj> children = default,
            bool inactive = default,
            bool @static = default,
            object key = default,
            ObjRef objRef = default
        ) {
            Name = string.IsNullOrEmpty(name) ? $"Obj{++LastIndex}" : name;
            Inactive = inactive;
            Static = @static;
            Components = components.ToDict(c => c.GetType(), c => c);
            Children = children.ToDict(c => c.Name, c => c);
            Key = key;
            Ref = objRef;
        }

        public override string ToString() {
            return $"Obj {Name} [Children={Children.Count}] [Components={Components.Count}]";
        }
    }
}