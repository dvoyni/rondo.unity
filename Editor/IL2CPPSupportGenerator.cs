using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Rondo.Core.Extras;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using Rondo.Unity.Components;
using UnityEditor;
using UnityEngine;
using Collider = Rondo.Unity.Components.Collider;

namespace Rondo.Unity {
    public static class IL2CPPSupportGenerator {
        [MenuItem("Rondo/Generate il2cpp support")]
        public static void GenerateIL2CPPSupport() {
            var csPaths = Directory.GetFiles(Application.dataPath, "*.rondo.cs", SearchOption.AllDirectories);

            if (csPaths.Length == 0) {
                EditorUtility.DisplayDialog(
                    "Failed to generate il2cpp support",
                    "Cannot find any *.rondo.cs file",
                    "Ok");
                return;
            }

            var sb = new StringBuilder();

            foreach (var csPath in csPaths) {
                var lines = File.ReadAllLines(csPath);
                if (lines.Length == 0) {
                    EditorUtility.DisplayDialog(
                        "Failed to generate il2cpp support",
                        $"{csPath} file is empty\n"
                        + "Should at least contain a line with game assemblies path prepended with //\n"
                        + " Multiple assemblies may be separated with ,\n"
                        + "E.g.:\n"
                        + "//MyApp.GameLogic, MyApp.Core",
                        "Ok");
                    continue;
                }

                var assemblies = lines[0].TrimStart('/').Split(',').Select(t => t.Trim() + ",").SelectMany(
                    t => AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(assembly => assembly.FullName.StartsWith(t))
                ).ToList();

                HashSet<Type> structs = new();
                HashSet<Type> ls = new();
                HashSet<Type> ds = new();
                Dictionary<Type, Dictionary<string, int>> offsets = new();
                Dictionary<Type, int> szs = new();
                var types = new List<Type>();
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(
                    a => a.FullName.StartsWith("Dvoyni.Rondo")
                ));

                types.AddRange(assemblies.SelectMany(
                    a => a.GetTypes().Where(t => t.IsValueType && !t.IsEnum && !t.IsPrimitive)
                ));
                types.AddRange(new[] {
                        typeof(IntPtr), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
                        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
                });
                foreach (var type in types) {
                    AddType(type, ref structs, ref ls, ref ds, ref szs);
                    AddOffsets(type, ref offsets);
                }

                sb.Clear();
                sb.AppendLine(lines[0]);
                sb.AppendLine("using Rondo.Core.Memory;");
                sb.Append("public static class AssemblyRegistry {");
                sb.AppendLine("public static void Attach() {");
                foreach (var (type, p) in offsets) {
                    foreach (var (field, offset) in p) {
                        sb.Append("Mem.__RegisterOffsetOf<").AppendType(type).Append(">(\"").Append(field).Append("\", ").Append(offset.ToString(CultureInfo.InvariantCulture)).AppendLine(");");
                    }
                }
                foreach (var (type, size) in szs) {
                    sb.Append("Mem.__RegisterSizeOf(typeof(").AppendType(type).Append("), ").Append(size.ToString(CultureInfo.InvariantCulture)).AppendLine(");");
                }
                sb.AppendLine("}");
                sb.AppendLine("public static void __ProduceGeneric() {");
                foreach (var t in structs) {
                    sb.Append("Serializer.__ProduceGenericStruct<").AppendType(t).AppendLine(">();");
                }
                foreach (var t in ls) {
                    sb.Append("Serializer.__ProduceGenericL<").AppendType(t.GetGenericArguments()[0]).AppendLine(">();");
                }
                foreach (var t in ds) {
                    var arg = t.GetGenericArguments();
                    sb.Append("Serializer.__ProduceGenericD<").AppendType(arg[0]).Append(",").AppendType(arg[1]).AppendLine(">();");
                }
                sb.AppendLine("}");
                sb.AppendLine("}");

                File.WriteAllText(csPath, sb.ToString());
            }
            EditorUtility.DisplayDialog(
                "Generation of il2cpp support",
                "Completed successfully",
                "Ok");
        }

        private static StringBuilder AppendType(this StringBuilder sb, Type t) {
            if (t.IsGenericType) {
                sb.Append(t.FullName.Split('`')[0].Replace('+', '.'));
                var args = t.GetGenericArguments();
                sb.Append("<");
                for (var index = 0; index < args.Length; index++) {
                    var a = args[index];
                    if (index != 0) {
                        sb.Append(",");
                    }
                    sb.AppendType(a);
                }
                sb.Append(">");
            }
            else {
                sb.Append(t.FullName.Split('`')[0].Replace('+', '.'));
            }
            return sb;
        }

        private static void AddType(
            Type t, ref HashSet<Type> structs, ref HashSet<Type> ls, ref HashSet<Type> ds, ref Dictionary<Type, int> szs
        ) {
            if (t.Name.EndsWith("*") && t.Name != "Void*") {
                szs[t] = Marshal.SizeOf(typeof(IntPtr));
            }
            if (
                t.Name == "MonoFNPtrFakeClass"
                || t.Name == "Void"
                || t.Name.EndsWith("*")
                || t.IsGenericParameter
                || !t.IsUnmanaged()
                || (t.IsGenericType && t.GetGenericArguments().Any(a => a.IsGenericParameter))
            ) {
                return;
            }
            if (t.IsEnum) {
                szs[t] = Marshal.SizeOf(t.GetEnumUnderlyingType());
            }
            else {
                szs[t] = Marshal.SizeOf(t);
            }

            var isContainer = false;
            if (t.IsGenericType) {
                var genericType = t.GetGenericTypeDefinition();
                if (genericType == typeof(L<>)) {
                    ls.Add(t);
                    isContainer = true;
                }
                else if (genericType == typeof(D<,>)) {
                    ds.Add(t);
                    isContainer = true;
                }
                else {
                    structs.Add(t);
                }
                var args = t.GetGenericArguments();
                foreach (var type in args) {
                    AddType(type, ref structs, ref ls, ref ds, ref szs);
                }
            }

            if (t.IsValueType && !t.IsEnum && !t.IsPrimitive && !isContainer) {
                if (structs.Add(t)) {
                    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var fi in fields) {
                        AddType(fi.FieldType, ref structs, ref ls, ref ds, ref szs);
                    }
                }
            }
        }

        private static void AddOffsets(Type t, ref Dictionary<Type, Dictionary<string, int>> offsets) {
            if (offsets.ContainsKey(t)) {
                return;
            }

            if (t.IsGenericParameter) {
                return;
            }

            if (t.IsGenericType) {
                var args = t.GetGenericArguments();
                var gp = false;
                foreach (var type in args) {
                    AddOffsets(type, ref offsets);
                    gp |= type.IsGenericParameter;
                }
                if (gp) {
                    return;
                }
            }

            if (t.IsValueType && !t.IsEnum && !t.IsPrimitive) {
                var d = new Dictionary<string, int>();
                offsets[t] = d;
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var fi in fields) {
                    d[fi.Name] = (int)Marshal.OffsetOf(t, fi.Name);
                    AddOffsets(fi.FieldType, ref offsets);
                }
            }
        }
    }
}