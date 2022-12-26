// using System;
// using System.IO;
// using Rondo.Core;
// using Rondo.Core.Lib;
// using UnityEngine;
//
// namespace Rondo.Unity.Cmds {
//     public static  class IO {
//         public readonly struct Buffer {
//             public readonly byte* Data;
//             public readonly int Size;
//
//             public Buffer(byte* data, int size) {
//                 Data = data;
//                 Size = size;
//             }
//         }
//
//         public static ICmd GetRecord<TMsg>(string key, Xf<Result<Buffer>, TMsg> toMsg)
//                 where TMsg : unmanaged {
//             static void Impl(Ptr pPayload, Xf<Ptr, Ptr> toMsg, PostMessage post) {
//                 Ptr arg;
//                 try {
//                     var path = Path.Combine(Application.persistentDataPath, (string)*pPayload.Cast<S>());
//                     var bytes = File.ReadAllBytes(path);
//                     fixed (byte* data = bytes) {
//                         var buf = new Buffer((byte*)Mem.C.CopyPtr(data, bytes.Length).Raw, bytes.Length);
//                         arg = Mem.C.CopyPtr(Result<Buffer>.Ok(buf));
//                     }
//                 }
//                 catch (Exception ex) {
//                     arg = Mem.C.CopyPtr(Result<Buffer>.Exception(ex));
//                 }
//                 post(toMsg, arg);
//                 toMsg.Dispose();
//             }
//
//             return Cmd<>.New(&Impl, toMsg, (S)key);
//         }
//
//         public readonly struct PutRecordPayload {
//             public readonly S Key;
//             public readonly Buffer Buffer;
//
//             public PutRecordPayload(S key, Buffer buffer) {
//                 Key = key;
//                 Buffer = buffer;
//             }
//         }
//
//         public static Cmd PutRecord<TMsg>(string key, Buffer buf, Xf<Result<int>, TMsg> toMsg)
//                 where TMsg : unmanaged {
//             static void Impl(Ptr pPayload, Xf<Ptr, Ptr> toMsg, PostMessage post) {
//                 Ptr arg;
//                 try {
//                     var payload = pPayload.Cast<PutRecordPayload>();
//                     var path = Path.Combine(Application.persistentDataPath, (string)payload->Key);
//                     var data = new Span<byte>(payload->Buffer.Data, payload->Buffer.Size);
//                     File.WriteAllBytes(path, data.ToArray());
//                     arg = Mem.C.CopyPtr(Result<int>.Ok(data.Length));
//                 }
//                 catch (Exception ex) {
//                     arg = Mem.C.CopyPtr(Result<int>.Exception(ex));
//                 }
//
//                 post(toMsg, arg);
//                 toMsg.Dispose();
//             }
//
//             return Cmd.New(&Impl, toMsg, Mem.C.CopyPtr(new PutRecordPayload((S)key, buf)));
//         }
//     }
// }