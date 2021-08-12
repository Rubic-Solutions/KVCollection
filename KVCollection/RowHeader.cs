using System;
using System.Diagnostics;
using System.IO;

namespace KeyValue
{
    public class RowHeader
    {
        public string PrimaryKey;
        public int KeySize => KeyActualLength;
        public int ValueSize => ValueActualLength;
        public int RowSize => Size + KeyLength + ValueLength;

        /// <summary>first start position in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long Pos;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long PrevPos;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long NextPos;

        /// <summary>first start position of the KEYs in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long KeyStartPos => Pos + Size;
        /// <summary>max value length of the KEYs</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal short KeyLength => 32;
        /// <summary>Actual value length of the KEYs</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal short KeyActualLength;

        /// <summary>first start position of the VALUE in stream</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal long ValueStartPos => Pos + Size + KeyLength;
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


        internal static long GetPointer(long startPos, RowHeaderPointers pointer) => startPos + (int)pointer;

        internal byte[] Data;
        public int FillBytes(Stream s)
        {
            Data = new byte[Size + KeyLength];
            return s.Read(Data);
        }
        public long GetPrevPos => BitConverter.ToInt64(Data, 0);
        public long GetNextPos => BitConverter.ToInt64(Data, 8);
        public short GetKeyActualLength => BitConverter.ToInt16(Data, 16);
        public int GetValueLength => BitConverter.ToInt32(Data, 18);
        public int GetValueActualLength => BitConverter.ToInt32(Data, 22);
        public string GetPrimaryKey => System.Text.Encoding.UTF8.GetString(Data, Size, GetKeyActualLength);
    }
    internal enum RowHeaderPointers
    {
        PrevPos = 0,
        NextPos = 8,
        KeyActualLength = 16,
        ValueLength = 18,
        ValueActualLength = 22
    }
}