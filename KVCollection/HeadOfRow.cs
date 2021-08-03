using System;

namespace KeyValueFile
{
    internal class HeadOfRow : Head<HeadOfRow>
    {
        public const int KeyLength = 32;
        public string Key;

        public long PrevPos;
        public long NextPos;

        /// <summary>Actual value length of the row</summary>
        public int ValueLength;
        /// <summary>max value length of the row</summary>
        public int RowLength;


        public override int Size => 8 + 8 + 4 + 4 + KeyLength; // ToArray().Length;
        public override byte[] ToArray() =>
            base.SerializeBytes(BitConverter.GetBytes(PrevPos),
                                BitConverter.GetBytes(NextPos),
                                BitConverter.GetBytes(ValueLength),
                                BitConverter.GetBytes(RowLength),
                                System.Text.Encoding.UTF8.GetBytes(Key));

        /// PrevPos(8)    NextPos(8)    ValueLength(4)    RowLength(4)    Key(32)
        /// -----------   -----------   ---------------   -------------   ----------- 
        /// 00 ... 07     08 ... 15     16 ... 19         20 ... 23       24 ... 55
        

        public override bool FromArray(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            this.PrevPos = BitConverter.ToInt64(data, 0);
            this.NextPos = BitConverter.ToInt64(data, 8);
            this.ValueLength = BitConverter.ToInt32(data, 16);
            this.RowLength = BitConverter.ToInt32(data, 20);

            var key_bytes = new byte[KeyLength];
            Array.Copy(data, 24, key_bytes, 0, KeyLength);
            this.Key = System.Text.Encoding.UTF8.GetString(key_bytes).TrimEnd(new char[] { (char)0 });
            return true;
        }
    }
}