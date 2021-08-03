using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace KeyValueFile
{
    internal class CollectionWriter
    {
        public readonly bool HasLog;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private FileStream dat = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private FileStream log = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private object lck = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<item> writingBuffers = new List<item>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool writingHasBegun;

        public CollectionWriter(System.IO.FileInfo File, bool NoLog = false)
        {
            this.HasLog = true; // !NoLog;

            dat = new FileStream(File.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (this.HasLog)
            {
                log = new FileStream(File.FullName + ".log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                if (wal_deserialize())
                    FlushToDisk(true);
            }
        }

        #region "Open/Close"
        public void Close()
        {
            dat.Close();
            log.Close();
        }
        #endregion

        #region "File methods"
        public long Length => dat.Length;
        public bool IsInitial => dat.Length == 0;
        public void WriteBegin(Action fn)
        {
            lock (lck)
            {
                writingBuffers.Clear();

                if (wal_deserialize()) FlushToDisk(true);
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
                FlushToDisk();
            }
        }
        private void FlushToDisk(bool recovery = false)
        {
            if (writingBuffers.Count == 0) return;

            if (recovery == false)
                wal_serialize();

            //execute commands
            foreach (var buffer in writingBuffers)
            {
                dat.Position = buffer.position ;
                dat.Write(buffer.data, 0, buffer.data.Length);
            }
            dat.Flush();

            wal_clear();
        }
        public void Write(long pos, byte[] data)
        {
            if (writingHasBegun == false)
                throw new Exception("[Write] method can be use in [WriteBegin] scope. [WriteBegin] must be call before.");
            if (data == null || data.Length == 0) return;
            writingBuffers.Add(new item() { position = pos, data = data });
        }
        public void Truncate()
        {
            dat.SetLength(0);
            dat.Flush();
        }
        #endregion

        #region "log (WAL) methods"
        private void wal_serialize()
        {
            if (this.HasLog == false) return;
            try
            {
                var data = new List<string>();
                foreach (var item in writingBuffers)
                {
                    data.Add(item.position.ToString());
                    data.Add(Convert.ToBase64String(item.data));
                }
                var bytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", data));
                var len = BitConverter.GetBytes(bytes.Length);
                log.Position = 0;
                log.Write(len, 0, len.Length);
                log.Write(bytes, 0, bytes.Length);
                log.Flush();
            }
            catch (Exception)
            {
                wal_clear();
                throw;
            }
        }

        private bool wal_deserialize()
        {
            if (this.HasLog == false) return false;
            if (log.Length == 0) return false;

            log.Position = 0;
            var len_bytes = new byte[4];
            if (log.Read(len_bytes, 0, len_bytes.Length) < len_bytes.Length) return false;
            var len = BitConverter.ToInt32(len_bytes, 0);
            if (len == 0) return false;

            var wal_bytes = new byte[len];
            log.Read(wal_bytes, 0, wal_bytes.Length);
            long key = -1;
            foreach (var item in System.Text.Encoding.UTF8.GetString(wal_bytes).Split(','))
            {
                if (key == -1)
                {
                    key = long.Parse(item);
                    continue;
                }

                writingBuffers.Add(new item() { position = key, data = Convert.FromBase64String(item) });
                key = -1;
            }

            return writingBuffers.Count > 0;
        }
        private byte[] wal_zero = BitConverter.GetBytes(0);
        private void wal_clear()
        {
            if (this.HasLog == false) return;

            log.Position = 0;
            log.Write(wal_zero, 0, 4);
            //wal.SetLength(0);   // it takes +2sec for 100K 
            log.Flush();
        }
        #endregion

        private class item
        {
            public long position;
            public byte[] data;
        }
    }
}