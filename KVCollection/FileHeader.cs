using System;
using System.Diagnostics;

namespace KeyValue
{
    internal class FileHeader
    {
        internal long Pos => 0;
        internal string Prefix => "KV";     // 2 byte
        internal int Version => 2210729;    // 4 byte
        internal long Count;                // 8 byte
        internal int LastId;                // 4 byte
        internal const int Size = 2 + 4 + 8 + 4;

        internal byte[] ToArray() =>
            Serializer.ConcatBytes(Size,
                System.Text.Encoding.ASCII.GetBytes(Prefix),
                BitConverter.GetBytes(Version),
                BitConverter.GetBytes(Count),
                BitConverter.GetBytes(LastId));

        /// POS=0  ->   Len=30  
        /// 
        /// Pfx     Vers(4)     Count(8)    LastId(4)
        /// -----   ---------   ---------   ---------
        /// 00 01   02 ... 05   06 ... 13   14 ... 17

        internal bool FromArray(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            var pfx_bytes = new byte[2];
            Buffer.BlockCopy(data, 0, pfx_bytes, 0, pfx_bytes.Length);

            var version = BitConverter.ToInt32(data, 2);
            if (System.Text.Encoding.ASCII.GetString(pfx_bytes) != Prefix ||
                version != Version) return false;

            // ...
            this.Count = BitConverter.ToInt64(data, 6);
            this.LastId = BitConverter.ToInt32(data, 14);
            return true;
        }
    }
}