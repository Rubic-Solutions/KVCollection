using System;
using System.IO;
using System.Linq;

namespace KeyValue
{
    public class RowHeader
    {
        /// <summary>position of the RowHeader at index-file</summary>
        public long Pos;
        public int Id;
        /// <summary>max value length of the row</summary>
        public int ValueActualSize;
        public long ValuePos;
        /// <summary>Actual value length of the row</summary>
        public int ValueSize;
        public object[] IndexValues;

        internal const int Size = 255;
        internal const int SizeOfIndex = Size - (4 + 4 + 4 + 8);

        internal void SetDeleted() => this.Id = 0;
        internal bool IsDeleted => this.Id == 0;

        internal bool FromArray(byte[] data, int start = 0)
        {
            this.Id = BitConverter.ToInt32(data, start);                    // 4 bytes
            if (this.Id > 0) // is it i not deleted
            {
                this.ValueSize = BitConverter.ToInt32(data, start + 4);         // 4 bytes
                this.ValueActualSize = BitConverter.ToInt32(data, start + 8);   // 4 bytes
                this.ValuePos = BitConverter.ToInt64(data, start + 12);          // 8 bytes
                this.IndexValues = Serializer.GetObjects(data, start + 20, SizeOfIndex).ToArray();
            }
            return true;
        }
        internal byte[] ToArray()
        {
            if (Id == 0)
                return Serializer.ConcatBytes(Size, BitConverter.GetBytes(Id));
            else
                return Serializer.ConcatBytes(Size,
                                              BitConverter.GetBytes(Id),                  // 4 bytes
                                              BitConverter.GetBytes(ValueSize),           // 4 bytes
                                              BitConverter.GetBytes(ValueActualSize),     // 4 bytes
                                              BitConverter.GetBytes(ValuePos),            // 8 bytes
                                              Serializer.GetBytes(IndexValues));
        }

    }
}