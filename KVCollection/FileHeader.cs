using System;
using System.Diagnostics;

namespace KeyValue
{
    internal class FileHeader
    {
        internal string Prefix => "KV";
        internal int Version => 2210729;
        internal long Pos => 0;
        internal long Count;
        internal long FirstRecStartPos;
        internal long LastRecStartPos;
        internal int Size => 2 + 4 + 8 + 8 + 8;

        public FileHeader()
        {
            this.FirstRecStartPos = Size;
            this.LastRecStartPos = Size;
        }
        internal byte[] ToArray() =>
            Serializer.ConcatBytes(this.Size,
                                System.Text.Encoding.ASCII.GetBytes(Prefix),
                                BitConverter.GetBytes(Version),
                                BitConverter.GetBytes(FirstRecStartPos),
                                BitConverter.GetBytes(LastRecStartPos),
                                BitConverter.GetBytes(Count));

        /// POS=0  ->   Len=30  
        /// 
        /// Pfx     Vers(4)     FirstRecStartPos(8)   LastRecStartPos(8)   Count(8)
        /// -----   ---------   -------------------   ------------------   ---------
        /// 00 01   02 ... 05   06 ... 13             14 ... 21            22 ... 29

        internal bool FromArray(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            var pfx_bytes = new byte[2];
            Array.Copy(data, 0, pfx_bytes, 0, pfx_bytes.Length);

            var version = BitConverter.ToInt32(data, 2);
            if (System.Text.Encoding.ASCII.GetString(pfx_bytes) != Prefix ||
                version != Version) return false;

            // ...
            this.FirstRecStartPos = BitConverter.ToInt64(data, 6);
            this.LastRecStartPos = BitConverter.ToInt64(data, 14);
            this.Count = BitConverter.ToInt64(data, 22);
            return true;
        }
    }
}