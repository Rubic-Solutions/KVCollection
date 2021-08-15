using System;
using System.IO;

namespace KeyValue
{
    public class RowHeader
    {
        //---------------------------------------------------------------
        //-- STATIC / CONST
        /// <summary>Size of pointers.</summary>
        internal static int RootSize = 26; //8 + 8 + 2 + 4 + 4;
        /// <summary>buffer size of Key</summary>
        internal static short KeySize = 32;
        internal static int SizeWithKey = RootSize + KeySize;

        //---------------------------------------------------------------
        /// <summary>first start position in stream</summary>
        public long RootPos;
        /// <summary>first start position of the KEYs in stream</summary>
        internal long KeyPos => RootPos + RootSize;
        /// <summary>first start position of the VALUE in stream</summary>
        internal long ValuePos => RootPos + SizeWithKey;

        public int RowSize => SizeWithKey + ValueSize;

        //---------------------------------------------------------------
        //-- DATA
        internal long PrevPos;
        internal long NextPos;
        /// <summary>Actual value length of the KEYs</summary>
        internal short KeyActualSize;
        /// <summary>Actual value length of the row</summary>
        public int ValueSize;
        /// <summary>max value length of the row</summary>
        public int ValueActualSize;
        public string PrimaryKey;
        //---------------------------------------------------------------

        internal byte[] ToArray(bool withPrimaryKey)
        {
            var key = System.Text.Encoding.UTF8.GetBytes(PrimaryKey);
            this.KeyActualSize = (short)key.Length;
            var retval = Serializer.ConcatBytes(SizeWithKey,
                                                BitConverter.GetBytes(PrevPos),             // 8
                                                BitConverter.GetBytes(NextPos),             // 8
                                                BitConverter.GetBytes(KeyActualSize),     // 2
                                                BitConverter.GetBytes(ValueSize),         // 4
                                                BitConverter.GetBytes(ValueActualSize),
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
            this.KeyActualSize = BitConverter.ToInt16(data, 16);      // +8
            this.ValueSize = BitConverter.ToInt32(data, 18);          // +2
            this.ValueActualSize = BitConverter.ToInt32(data, 22);    // +4
            this.PrimaryKey = System.Text.Encoding.UTF8.GetString(data, 26, this.KeyActualSize);    // +32

            return true;
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