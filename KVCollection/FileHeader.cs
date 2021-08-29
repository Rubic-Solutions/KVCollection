using System;

namespace KV
{
    internal enum FileState
    {
        Normal = 0,
        Shrink = 1
    }

    internal class FileHeader
    {
        internal long Pos => 0;
        internal string Prefix => "KV";     // 2 byte
        internal int Version => 2210729;    // 4 byte
        internal long Count;                // 8 byte
        internal int LastId;                // 4 byte
        internal FileState State;            // 4 byte (0:Normal, 1:Shrink, n:...)
        internal const int Size = 255;

        internal byte[] ToArray() =>
            Serializer.ConcatBytes(Size,
                System.Text.Encoding.ASCII.GetBytes(Prefix),
                BitConverter.GetBytes(Version),
                BitConverter.GetBytes(Count),
                BitConverter.GetBytes(LastId),
                BitConverter.GetBytes((int)State));

        /// POS=0  ->   Len=30  
        /// 
        /// Pfx     Vers(4)     Count(8)    LastId(4)  Mode(2)
        /// -----   ---------   ---------   ---------  ---------
        /// 00 01   02 ... 05   06 ... 13   14 ... 17  18 ... 19

        internal bool FromArray(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            var pfx_bytes = new byte[2];
            Array.Copy(data, 0, pfx_bytes, 0, pfx_bytes.Length);

            var version = BitConverter.ToInt32(data, 2);
            if (System.Text.Encoding.ASCII.GetString(pfx_bytes) != Prefix ||
                version != Version) return false;

            // ...
            this.Count = BitConverter.ToInt64(data, 6);
            this.LastId = BitConverter.ToInt32(data, 14);
            this.State = (FileState)BitConverter.ToInt32(data, 18);
            return true;
        }
    }
}