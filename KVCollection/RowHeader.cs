using System;
using System.IO;

namespace KeyValue
{
    public class RowHeader
    {
        /// <summary>first start position in stream</summary>
        public long Pos;
        /// <summary>first start position of the KEYs in stream</summary>
        internal long KeyStartPos => Pos + Size;
        /// <summary>first start position of the VALUE in stream</summary>
        internal long ValueStartPos => Pos + Size + KeyLength;

        public int RowSize => Size + KeyLength + ValueLength;
        /// <summary>Size of ToArray output.</summary>
        private int Size => 26; //8 + 8 + 2 + 4 + 4;
        /// <summary>max value length of the KEYs</summary>
        private short KeyLength => 32;
        //---------------------------------------------------------------
        //-- DATA
        internal long PrevPos;
        internal long NextPos;
        /// <summary>Actual value length of the KEYs</summary>
        internal short KeyActualLength;
        /// <summary>Actual value length of the row</summary>
        internal int ValueLength;
        /// <summary>max value length of the row</summary>
        internal int ValueActualLength;
        public string PrimaryKey;
        //---------------------------------------------------------------

        internal byte[] ToArray(bool withPrimaryKey)
        {
            var key = System.Text.Encoding.UTF8.GetBytes(PrimaryKey);
            this.KeyActualLength = (short)key.Length;
            var retval = Serializer.ConcatBytes(this.Size + this.KeyLength,
                                                BitConverter.GetBytes(PrevPos),             // 8
                                                BitConverter.GetBytes(NextPos),             // 8
                                                BitConverter.GetBytes(KeyActualLength),     // 2
                                                BitConverter.GetBytes(ValueLength),         // 4
                                                BitConverter.GetBytes(ValueActualLength),
                                                key);  // 4
            return retval;
        }

        /// POS=n  ->   Len=256  
        /// 
        /// PrevPos(8)    NextPos(8)    ValueLength(4)    RowLength(4) 
        /// -----------   -----------   ---------------   -------------
        /// 00 ... 07     08 ... 15     16 ... 19         20 ... 23    


        internal bool FromArray(byte[] data)
        {
            this.PrevPos = BitConverter.ToInt64(data, 0);               //  0
            this.NextPos = BitConverter.ToInt64(data, 8);               // +8
            this.KeyActualLength = BitConverter.ToInt16(data, 16);      // +8
            this.ValueLength = BitConverter.ToInt32(data, 18);          // +2
            this.ValueActualLength = BitConverter.ToInt32(data, 22);    // +4
            this.PrimaryKey = System.Text.Encoding.UTF8.GetString(data, 26, this.KeyActualLength);    // +32

            return true;
        }


        public void FillBytes(Stream s)
        {
            var bytes = new byte[Size + KeyLength];
            var retval = s.Read(bytes);
            FromArray(bytes);
        }
        //internal static long GetPointer(long startPos, RowHeaderPointers pointer) => startPos + (int)pointer;
    }

    //internal enum RowHeaderPointers
    //{
    //    PrevPos = 0,
    //    NextPos = 8,
    //    KeyActualLength = 16,
    //    ValueLength = 18,
    //    ValueActualLength = 22,
    //    PrimaryKey = 26
    //}
}