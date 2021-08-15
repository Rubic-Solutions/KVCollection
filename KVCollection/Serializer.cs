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

    }
}