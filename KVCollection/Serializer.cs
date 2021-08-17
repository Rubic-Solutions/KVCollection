using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace KeyValue
{
    public static class Serializer
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static System.Text.Encoding enc = System.Text.Encoding.UTF8;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static JsonSerializerOptions serialize_opt = new JsonSerializerOptions()
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IgnoreNullValues = true
        };

        public static byte[] GetBytes(string obj) => enc.GetBytes(obj);
        public static byte[] GetBytes<K>(K obj) => enc.GetBytes(ToJson(obj));

        public static string GetString(byte[] bytes) => GetString(bytes, 0, bytes.Length);
        public static string GetString(byte[] bytes, int index, int count) => (bytes == null || bytes.Length == 0) ? default : enc.GetString(bytes, index, count);

        public static T GetObject<T>(byte[] bytes) => GetObject<T>(bytes, 0, bytes.Length);
        internal static T GetObject<T>(byte[] bytes, int index, int count) => (bytes == null || bytes.Length == 0) ? default : FromJson<T>(enc.GetString(bytes, index, count));

        public static string ToJson(object obj) => JsonSerializer.Serialize(obj, serialize_opt);
        public static T FromJson<T>(string data) => JsonSerializer.Deserialize<T>(data, serialize_opt);


        internal static byte[] ConcatBytes(int Size, params byte[][] datas)
        {
            var retval = new byte[Size];
            int p = 0;
            foreach (var data in datas)
            {
                System.Array.Copy(data, 0, retval, p, data.Length);
                p += data.Length;
            }
            return retval;
        }


        public static byte[] GetBytes(params object[] vals)
        {
            var retval = new List<byte>(1024);

            var pos = 0;
            void addBytes(byte typ, byte[] val)
            {
                byte len = (byte)val.Length;
                if (len > 0)
                {
                    retval.Add(typ);
                    pos++;
                    //if (typ == 1)   // only for string
                    //{
                    retval.Add(len);
                    pos++;
                    //}
                    for (int i = 0; i < len; i++)
                        retval.Add(val[i]);

                    pos += len;
                }

            }
            var enc = System.Text.Encoding.UTF8;
            foreach (var item in vals)
            {
                Type t = item.GetType();
                if (t == typeof(string))
                    addBytes(1, enc.GetBytes((string)item));
                else if (t == typeof(bool))
                    addBytes(2, BitConverter.GetBytes((bool)item));
                else if (t == typeof(short))
                    addBytes(3, BitConverter.GetBytes((short)item));
                else if (t == typeof(ushort))
                    addBytes(4, BitConverter.GetBytes((ushort)item));
                else if (t == typeof(int))
                    addBytes(5, BitConverter.GetBytes((int)item));
                else if (t == typeof(uint))
                    addBytes(6, BitConverter.GetBytes((uint)item));
                else if (t == typeof(long))
                    addBytes(7, BitConverter.GetBytes((long)item));
                else if (t == typeof(ulong))
                    addBytes(8, BitConverter.GetBytes((ulong)item));
                else if (t == typeof(Single))
                    addBytes(9, BitConverter.GetBytes((Single)item));
                else if (t == typeof(double))
                    addBytes(10, BitConverter.GetBytes((double)item));
                else if (t == typeof(decimal))
                    addBytes(11, GetBytes((decimal)item));
                else if (t == typeof(DateTime))
                    addBytes(12, BitConverter.GetBytes(((DateTime)item).Ticks));
            }
            return retval.ToArray();
        }
        public static IEnumerable<object> GetObjects(byte[] bytes,int start = 0, int count=-1)
        {
            var enc = System.Text.Encoding.UTF8;
            if (count == -1) count = bytes.Length;
            for (int pos = start; pos < count; pos++)
            {
                byte typ = bytes[pos];
                if (typ == 0) break;

                pos++;
                var len = bytes[pos];
                if (len == 0) break;

                pos++;
                if (typ == 1)
                    yield return enc.GetString(bytes, pos, len);
                if (typ == 2)
                    yield return BitConverter.ToBoolean(bytes, pos);
                if (typ == 3)
                    yield return BitConverter.ToInt16(bytes, pos);
                if (typ == 4)
                    yield return BitConverter.ToUInt16(bytes, pos);
                if (typ == 5)
                    yield return BitConverter.ToInt32(bytes, pos);
                if (typ == 6)
                    yield return BitConverter.ToUInt32(bytes, pos);
                if (typ == 7)
                    yield return BitConverter.ToInt64(bytes, pos);
                if (typ == 8)
                    yield return BitConverter.ToUInt64(bytes, pos);
                if (typ == 9)
                    yield return BitConverter.ToSingle(bytes, pos);
                if (typ == 10)
                    yield return BitConverter.ToDouble(bytes, pos);
                if (typ == 11)
                    yield return getDecimalValue(bytes, pos);
                if (typ == 12)
                    yield return new DateTime(BitConverter.ToInt64(bytes, pos));

                pos += len - 1;
            }
        }

        private static byte[] getDecimalBytes(decimal dec)
        {
            Int32[] bits = decimal.GetBits(dec);
            List<byte> bytes = new List<byte>();
            foreach (Int32 i in bits)
                bytes.AddRange(BitConverter.GetBytes(i));
            
            return bytes.ToArray();
        }
        private static decimal getDecimalValue(byte[] bytes, int start = 0)
        {
            Int32[] bits = new Int32[4];
            for (int i = 0; i <= 15; i += 4)
                bits[i / 4] = BitConverter.ToInt32(bytes, start + i);

            return new decimal(bits);
        }

    }
}