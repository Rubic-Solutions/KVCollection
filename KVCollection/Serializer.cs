using System.Diagnostics;
using System.Text.Json;

namespace KeyValue
{
    internal class Serializer
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

        internal static byte[] ToBytes(object obj) => enc.GetBytes(ToString(obj));
        internal static T FromBytes<T>(byte[] bytes) => (bytes == null || bytes.Length == 0) ? default : FromString<T>(enc.GetString(bytes));
        internal static T FromBytes<T>(byte[] bytes, int index, int count) => (bytes == null || bytes.Length == 0) ? default : FromString<T>(enc.GetString(bytes, index, count));

        internal static string ToString(object obj) => JsonSerializer.Serialize(obj, serialize_opt);
        internal static T FromString<T>(string data) => JsonSerializer.Deserialize<T>(data, serialize_opt);


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