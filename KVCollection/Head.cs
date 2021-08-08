using System.Collections.Generic;

namespace KeyValue
{
    public abstract class Head<T>
    {
        internal long Pos;
        internal abstract int Size { get; }

        internal abstract byte[] ToArray();
        internal abstract bool FromArray(byte[] data);

        protected byte[] SerializeBytes(params byte[][] datas)
        {
            var retval = new byte[Size];
            int p=0;
            foreach (var data in datas)
            {
                System.Array.Copy(data, 0, retval, p, data.Length);
                p += data.Length;
            }
            return retval;
        }
    }
}