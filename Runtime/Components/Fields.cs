using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Rondo.Core.Extras;
using Rondo.Core.Lib;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rondo.Unity.Components {
    public static unsafe class Fields {
        private delegate void SetPtrValueDelegate(PropertyInfo pi, Component component, Ptr ptr);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<(Type, S), PropertyInfo> _propertyInfos = new();
        private static readonly Dictionary<Type, SetPtrValueDelegate> _setters = new();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly RefHash _refs = new();

        private readonly struct Data {
            public readonly Ts Type;
            public readonly D<S, Field> Fields;

            public Data(Ts type, D<S, Field> fields) {
                Type = type;
                Fields = fields;
            }
        }

        private static readonly ulong _idFields = CompExtensions.NextId;

        public static Comp New<T>(L<Field> fields) where T : Component {
            var data = new Data((Ts)typeof(T), fields.ToDMap(&NamedField));
            return new(_idFields, &SyncFields, Mem.C.CopyPtr(data));
        }

        private static void SyncFields(IPresenter presenter, GameObject gameObject, Ptr pPrev, Ptr pNext) {
            if (pNext == Ptr.Null) {
                var component = gameObject.GetComponent((Type)pPrev.Cast<Data>()->Type);
                if (!ReferenceEquals(component, null)) {
                    Object.Destroy(component);
                }
                return;
            }
            var next = *pNext.Cast<Data>();
            var prev = pPrev == Ptr.Null ? new Data(next.Type, new()) : *pPrev.Cast<Data>();
            prev.Fields.Merge(next.Fields, &SyncRemovedField, &SyncExistedField, &SyncNewField,
                new InterData(gameObject, next.Type));
        }

        private static P<S, Field> NamedField(Field f) {
            return new(f.Name, f);
        }

        private static InterData SyncRemovedField(S s, Field field, InterData x) {
            return x;
        }

        private static InterData SyncExistedField(S n, Field pf, Field nf, InterData x) {
            if (!nf.DeepEquals(pf)) {
                SetValue(x, n, nf);
            }
            return x;
        }

        private static InterData SyncNewField(S n, Field nf, InterData x) {
            SetValue(x, n, nf);
            return x;
        }

        private static void SetValue(InterData x, S name, Field field) {
            var component = x.Component;

            Type componentType;
            if (ReferenceEquals(component, null)) {
                componentType = (Type)x.Type;
                component = x.GameObject.GetComponent(componentType);
                if (!ReferenceEquals(component, null)) {
                    component = x.GameObject.AddComponent(componentType);
                }
            }
            else {
                componentType = component.GetType();
            }
            //try {
            if (!_propertyInfos.TryGetValue((componentType, name), out var propInfo)) {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
                foreach (var pi in componentType.GetProperties(flags)) {
                    if (pi.CanWrite) {
                        var n = (S)pi.Name;
                        _propertyInfos[(componentType, n)] = pi;

                        if (n == name) {
                            propInfo = pi;
                        }
                    }
                }
                if (propInfo == null) {
                    throw new FieldAccessException($"Component {componentType.Name} has no accessible property {(string)name}");
                }
            }

            switch (field.Kind) {
                case FieldKind.Ref:
                    propInfo.SetValue(component, Mem.C.GetObject<object>(field.Ref));
                    break;
                case FieldKind.Ptr:
                    var type = (Type)field.Ptr.Type;
                    if (!_setters.TryGetValue(type, out var setter)) {
                        var method = typeof(Fields).GetMethod(nameof(SetPtrValue));
                        if (method != null) {
                            setter = (SetPtrValueDelegate)method
                                    .MakeGenericMethod(type)
                                    .CreateDelegate(typeof(SetPtrValueDelegate));
                        }
                        _setters[type] = setter;
                    }
                    setter?.Invoke(propInfo, component, field.Ptr);
                    break;
                case FieldKind.S:
                    propInfo.SetValue(component, (string)field.S);
                    break;
                case FieldKind.Int:
                    propInfo.SetValue(component, field.Int);
                    break;
                case FieldKind.Float:
                    propInfo.SetValue(component, field.Float);
                    break;
                case FieldKind.Bool:
                    propInfo.SetValue(component, field.Bool);
                    break;
                case FieldKind.Prefab:
                    AddressablesCache.Load(
                        field.S, field.Type, x.GameObject,
                        HandlePrefabLoaded(_refs.Hash(component), _refs.Hash(propInfo))
                    );
                    break;
            }
            //}
            //catch {
            /*
             * In case of NotImplementedException being thrown.
             * For some reason specifying that exception didn't seem to catch it,
             * so I didn't catch anything specific.
             */
            //}
        }

        private static CLa<GameObject, Object> HandlePrefabLoaded(Ref componentRef, Ref propInfoRef) {
            static void Impl(GameObject gameObject, Object prefab, Ref* componentRef, Ref* propInfoRef) {
                var propInfo = _refs.Remove<PropertyInfo>(*propInfoRef);
                var component = _refs.Remove<Component>(*componentRef);
                propInfo.SetValue(component, prefab);
            }

            return CLa.New<GameObject, Object, Ref, Ref>(&Impl, componentRef, propInfoRef);
        }

        private static void SetPtrValue<T>(PropertyInfo pi, Component component, Ptr ptr) where T : unmanaged {
            pi.SetValue(component, *ptr.Cast<T>());
        }

        private readonly struct InterData {
            public readonly GameObject GameObject;
            public readonly Component Component;
            public readonly Ts Type;

            public InterData(GameObject gameObject, Ts type, Component component = null) {
                GameObject = gameObject;
                Component = component;
                Type = type;
            }
        }
    }

    internal enum FieldKind {
        Ref,
        Ptr,
        S,
        Int,
        Float,
        Bool,
        Prefab,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Field : IEquatable<Field> {
        [FieldOffset(0)] internal S Name;
        [FieldOffset(4)] internal FieldKind Kind;
        [FieldOffset(8)] internal Ts Type;
        [FieldOffset(12)] internal Ref Ref;
        [FieldOffset(12)] internal Ptr Ptr;
        [FieldOffset(12)] internal S S;
        [FieldOffset(12)] internal int Int;
        [FieldOffset(12)] internal float Float;
        [FieldOffset(12)] internal bool Bool;

        public Field(string name, object value)
                : this(name, Mem.C.HashObject(value)) {
            CheckUnmanaged(value);
        }

        public Field(string name, Ref value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Ref;
            Ref = value;
        }

        public Field Struct<T>(string name, T value) where T : unmanaged {
            return new Field(name, Mem.C.CopyPtr(value));
        }

        public Field(string name, Ptr value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Ptr;
            Ptr = value;
        }

        public Field(string name, int value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Int;
            Int = value;
        }

        public Field(string name, float value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Float;
            Float = value;
        }

        public Field(string name, bool value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Bool;
            Bool = value;
        }

        public Field(string name, string value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.S;
            S = (S)value;
        }

        public Field(string name, Type type, string value)
                : this() {
            Name = (S)name;
            Kind = FieldKind.Prefab;
            Type = (Ts)type;
            S = (S)value;
        }

        public bool DeepEquals(Field prev) {
            if (Kind != prev.Kind) {
                return false;
            }

            switch (Kind) {
                case FieldKind.Ref:
                    return Mem.C.GetObject<object>(Ref) == Mem.Prev.GetObject<object>(prev.Ref);
                case FieldKind.Ptr:
                    return Ptr == prev.Ptr;
                case FieldKind.S:
                    return S == prev.S;
                case FieldKind.Int:
                    return Int == prev.Int;
                case FieldKind.Float:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return Float == prev.Float;
                case FieldKind.Bool:
                    return Bool == prev.Bool;
                case FieldKind.Prefab:
                    return S == prev.S;
            }
            return false;
        }

        public bool Equals(Field other) {
            if (Kind != other.Kind) {
                return false;
            }

            switch (Kind) {
                case FieldKind.Ref:
                    return Ref.Equals(other.Ref);
                case FieldKind.Ptr:
                    return Ptr == other.Ptr;
                case FieldKind.S:
                    return S == other.S;
                case FieldKind.Int:
                    return Int == other.Int;
                case FieldKind.Float:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return Float == other.Float;
                case FieldKind.Bool:
                    return Bool == other.Bool;
                case FieldKind.Prefab:
                    return S == other.S;
            }
            return false;
        }

        public override bool Equals(object obj) {
            return obj is Field other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine((int)Kind, Ref, Ptr, S, Int, Float, Bool);
        }

        public static bool operator ==(Field a, Field b) {
            return a.Equals(b);
        }

        public static bool operator !=(Field a, Field b) {
            return !(a == b);
        }

        [Conditional("DEBUG")]
        private static void CheckUnmanaged(object value) {
            if ((value != null) && value.GetType().IsUnmanaged()) {
                Assert.Fail($"Use {nameof(Field)}.{nameof(Struct)}() instead of new Field() to create unmanaged struct field");
            }
        }
    }
}