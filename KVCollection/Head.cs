using System.Collections.Generic;

namespace KeyValueFile
{
    internal abstract class Head<T>
    {
        public long Pos;
        public abstract int Size { get; }
        public abstract byte[] ToArray();
        public abstract bool FromArray(byte[] data);

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