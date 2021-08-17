using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KeyValue
{
    internal class CollectionWriter
    {
        public readonly bool UseLog;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int InstanceCount;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal FileStream fs_inx = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal FileStream fs_dat = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private FileStream fs_log = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private object lck = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<item> writingBuffers = new List<item>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool writingHasBegun;

        public CollectionWriter(string Directory, string CollectionName, bool NoLog = false)
        {
            this.UseLog = true; // !NoLog;

            var name = System.IO.Path.Combine(Directory, CollectionName);
            fs_inx = new FileStream(name + ".inx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            fs_dat = new FileStream(name + ".dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (this.UseLog)
            {
                fs_log = new FileStream(name + ".log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

                if (wal_deserialize()) FlushToDisk(true);
            }
        }

        #region "Open/Close"
        public void Close()
        {
            fs_inx.Close();
            fs_dat.Close();
            fs_log.Close();
        }
        #endregion

        #region "File methods"
        public void WriteBegin(Action fn)
        {
            lock (lck)
            {
                //writingBuffers.Clear();

                //if (wal_deserialize()) FlushToDisk(true);
                if (writingBuffers.Count > 0) FlushToDisk(true);


                try
                {
                    writingHasBegun = true;
                    fn();
                }
                catch (Exception)
                {
                    writingHasBegun = false;
                    throw;
                }

                FlushToDisk(false);
            }
        }

        //private Stopwatch sw = new Stopwatch();
        private void FlushToDisk(bool recovery)
        {
            if (writingBuffers.Count == 0) return;

            if (recovery == false)
                wal_serialize();

            //sw.Restart();
            //execute commands
            foreach (var buffer in writingBuffers)
            {
                if (buffer.isValue == 1)
                {
                    if (fs_dat.Position != buffer.position)
                        fs_dat.Position = buffer.position;

                    fs_dat.Write(buffer.data);
                }
                else
                {
                    if (fs_inx.Position != buffer.position)
                        fs_inx.Position = buffer.position;

                    fs_inx.Write(buffer.data);
                }
            }
            //if (sw.Elapsed.Ticks > 2000)
            //    System.Diagnostics.Debug.Print(sw.Elapsed.Ticks.ToString());

            fs_inx.Flush();

            writingBuffers.Clear();

            //wal_clear();
        }
        public void Write(long pos, byte[] data, int IsValue)
        {
            if (writingHasBegun == false)
                throw new Exception("[Write] method can be use in [WriteBegin] scope. [WriteBegin] must be call before.");
            if (data == null || data.Length == 0) return;
            writingBuffers.Add(new item() { isValue = IsValue, position = pos, data = data });
        }

        public void Truncate()
        {
            fs_inx.SetLength(0);
            fs_inx.Flush();
            fs_dat.SetLength(0);
            fs_dat.Flush();
            fs_log.SetLength(0);
            fs_log.Flush();
        }
        #endregion

        #region "log (WAL) methods"
        private void wal_serialize()
        {
            if (this.UseLog == false) return;
            try
            {
                var items = new List<object>();
                foreach (var item in writingBuffers)
                {
                    items.Add(item.isValue);
                    items.Add(item.position);
                    items.Add(item.data);
                }
                var dat = Serializer.GetBytes(items.ToArray());
                var len = BitConverter.GetBytes((int)dat.Length);

                fs_log.Position = 0;
                fs_log.Write(Serializer.ConcatBytes(dat.Length + 4, len, dat));
                fs_log.Flush();

                wal_deserialize();
            }
            catch (Exception)
            {
                wal_clear();
                throw;
            }
        }
        private bool wal_deserialize()
        {
            if (this.UseLog == false) return false;
            if (fs_log.Length == 0) return false;

            fs_log.Position = 0;
            var len_bytes = new byte[4];
            if (fs_log.Read(len_bytes, 0, len_bytes.Length) < len_bytes.Length) return false;
            var len = BitConverter.ToInt32(len_bytes, 0);
            if (len == 0) return false;

            var wal_bytes = new byte[len];
            fs_log.Read(wal_bytes, 0, wal_bytes.Length);
            int partNo = 0;
            item item = null;
            foreach (var part in Serializer.GetObjects(wal_bytes))
            {
                if (partNo == 0)
                {
                    item = new item();
                    item.isValue = (int)part;
                    partNo++;
                }
                else if (partNo == 1)
                {
                    item.position = (long)part;
                    partNo++;
                }
                else
                {
                    item.data = (byte[])part;
                    writingBuffers.Add(item);
                    partNo = 0;
                }
            }

            return writingBuffers.Count > 0;
        }
        private byte[] wal_zero = BitConverter.GetBytes(0);
        private void wal_clear()
        {
            if (this.UseLog == false) return;

            fs_log.Seek(0, SeekOrigin.Begin);
            fs_log.Write(wal_zero, 0, 4);
            //wal.SetLength(0);   // it takes +2sec for 100K 
            fs_log.Flush();

            writingBuffers.Clear();
        }
        #endregion

        private class item
        {
            public int isValue;
            public long position;
            public byte[] data;
        }
    }
}