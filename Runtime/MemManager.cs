using System;
using System.Reflection;
using Rondo.Core.Memory;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Rondo.Unity {
    public unsafe class MemManager : IMemManager {
        public int SizeOf<T>() where T : struct {
            return UnsafeUtility.SizeOf<T>();
        }

        public int SizeOf(Type t) {
            return UnsafeUtility.SizeOf(t);
        }

        public int GetFieldOffset(FieldInfo fieldInfo) {
            return UnsafeUtility.GetFieldOffset(fieldInfo);
        }

        public void* Alloc(long size) {
            return UnsafeUtility.Malloc(size, 4, Allocator.Persistent);
        }

        public void Free(void* mem) {
            UnsafeUtility.Free(mem, Allocator.Persistent);
        }

        public void MemCpy(void* src, void* dst, long size) {
            UnsafeUtility.MemCpy(dst, src, size);
        }

        public bool MemCmp(void* a, void* b, long size) {
            return UnsafeUtility.MemCmp(a, b, size) == 0;
        }

        public bool IsUnmanaged(Type t) {
            return UnsafeUtility.IsUnmanaged(t);
        }
    }
}