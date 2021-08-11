using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KeyValue
{
    public class RowHeader
    {
        public string PrimaryKey => (string)Keys[0];
        public object IndexKey(int index) => Keys[index];
        public int IndexKeyCount => Keys.Count;
        public int KeySize => KeyActualLength;
        public int ValueSize => ValueActualLength;
        public int RowSize => Size + KeyLength + ValueLength;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal List<object> Keys = new List<object>(); // PrimaryKey + IndexKeys

        /// <summary>first start position in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long Pos;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long PrevPos;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long NextPos;

        /// <summary>first start position of the KEYs in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long PosKeyStart => Pos + Size;
        /// <summary>max value length of the KEYs</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal short KeyLength => 256;
        /// <summary>Actual value length of the KEYs</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal short KeyActualLength;

        /// <summary>first start position of the VALUE in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long PosValueStart => Pos + Size + KeyLength;
        /// <summary>Actual value length of the row</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int ValueLength;
        /// <summary>max value length of the row</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int ValueActualLength;


        /// <summary>Size of ToArray output.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int Size => 8 + 8 + 2 + 4 + 4;

        internal byte[] ToArray()
        {
            var retval = Serializer.ConcatBytes(this.Size,
                                                BitConverter.GetBytes(PrevPos),             // 8
                                                BitConverter.GetBytes(NextPos),             // 8
                                                BitConverter.GetBytes(KeyActualLength),     // 2
                                                BitConverter.GetBytes(ValueLength),         // 4
                                                BitConverter.GetBytes(ValueActualLength));  // 4
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

            return true;
        }
    }
}