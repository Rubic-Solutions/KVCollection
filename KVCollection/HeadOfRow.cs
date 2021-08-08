using System;
using System.Diagnostics;

namespace KeyValue
{
    public class HeadOfRow : Head<HeadOfRow>
    {
        public string PrimaryKey => Keys[0];
        public string Key2 => Keys[1];
        public string Key3 => Keys[2];
        public string Key4 => Keys[3];
        public string Key5 => Keys[4];

        internal long PrevPos;
        internal long NextPos;
        /// <summary>Actual value length of the row</summary>
        internal int ValueLength;
        /// <summary>max value length of the row</summary>
        internal int RowLength;
        internal string[] Keys = new string[5];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int adr_length = 8 + 8 + 4 + 4;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int key_length = ((5 * 32) + 4);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int full_length = adr_length + key_length;

        internal override int Size => full_length;
        internal override byte[] ToArray()
        {
            var retval = base.SerializeBytes(BitConverter.GetBytes(PrevPos),
                                             BitConverter.GetBytes(NextPos),
                                             BitConverter.GetBytes(ValueLength),
                                             BitConverter.GetBytes(RowLength),
                                             System.Text.Encoding.UTF8.GetBytes(PrimaryKey + "," +
                                                               Key2 + "," +
                                                               Key3 + "," +
                                                               Key4 + "," +
                                                               Key5));
            return retval;
        }
        /// PrevPos(8)    NextPos(8)    ValueLength(4)    RowLength(4)    Key [1-5]
        /// -----------   -----------   ---------------   -------------   ----------- 
        /// 00 ... 07     08 ... 15     16 ... 19         20 ... 23       24 ... nnn


        internal override bool FromArray(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            this.PrevPos = BitConverter.ToInt64(data, 0);
            this.NextPos = BitConverter.ToInt64(data, 8);
            this.ValueLength = BitConverter.ToInt32(data, 16);
            this.RowLength = BitConverter.ToInt32(data, 20);

            var key_bytes = new byte[key_length];
            Array.Copy(data, adr_length, key_bytes, 0, key_length);
            Keys = System.Text.Encoding.UTF8.GetString(key_bytes).Split(',');
            return true;
        }
    }
}