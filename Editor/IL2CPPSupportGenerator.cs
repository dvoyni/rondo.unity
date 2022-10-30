using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rondo.Core.Lib.Containers;
using Rondo.Core.Memory;
using UnityEditor;
using UnityEngine;

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
                        + "Should at least contain a line with game assembly names prepended with //\n"
                        + "Multiple assemblies may be separated with ,\n"
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
                    AddType(type, ref structs, ref ls, ref ds);
                }

                sb.Clear();
                sb.AppendLine(lines[0]);
                sb.AppendLine($"using {nameof(Rondo)}.{nameof(Rondo.Core)}.{nameof(Rondo.Core.Memory)};");
                sb.Append("public static class AssemblyRegistry").Append(Path.GetFileNameWithoutExtension(csPath).Split('.')[0]).AppendLine(" {");
                sb.AppendLine("public static void __ProduceGeneric  () {");
                foreach (var t in structs) {
                    sb.Append($"{nameof(Serializer)}.{nameof(Serializer.__ProduceGenericStruct)}<").AppendType(t).AppendLine(">();");
                }
                foreach (var t in ls) {
                    sb.Append($"{nameof(Serializer)}.{nameof(Serializer.__ProduceGenericA)}<").AppendType(t.GetGenericArguments()[0]).AppendLine(">();");
                }
                foreach (var t in ds) {
                    var arg = t.GetGenericArguments();
                    sb.Append($"{nameof(Serializer)}.{nameof(Serializer.__ProduceGenericD)}<").AppendType(arg[0]).Append(",").AppendType(arg[1]).AppendLine(">();");
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

        private static void AddType(Type t, ref HashSet<Type> structs, ref HashSet<Type> ls, ref HashSet<Type> ds) {
            if (
                t.Name == $"MonoFNPtrFakeClass"
                || t.Name == "Void"
                || t.Name.EndsWith("*")
                || t.IsGenericParameter
                || !IsUnmanaged(t)
                || (t.IsGenericType && t.GetGenericArguments().Any(a => a.IsGenericParameter))
            ) {
                return;
            }

            var isContainer = false;
            if (t.IsGenericType) {
                var genericType = t.GetGenericTypeDefinition();
                if (genericType == typeof(A<>)) {
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
                    AddType(type, ref structs, ref ls, ref ds);
                }
            }

            if (t.IsValueType && !t.IsEnum && !t.IsPrimitive && !isContainer) {
                if (structs.Add(t)) {
                    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var fi in fields) {
                        AddType(fi.FieldType, ref structs, ref ls, ref ds);
                    }
                }
            }
        }

        private static bool IsUnmanaged(Type t) {
            if (t.IsPrimitive || t.IsPointer || t.IsEnum) {
                return true;
            }
            if ( /*t.IsGenericType ||*/ !t.IsValueType) {
                return false;
            }
            return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.FieldType.Name != "MonoFNPtrFakeClass")
                    .All(x => IsUnmanaged(x.FieldType));
        }
    }
}