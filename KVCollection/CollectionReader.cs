using System;
using System.Collections.Generic;
using System.IO;

namespace KV
{
    internal class CollectionReader
    {
        private readonly Collection col;
        public CollectionReader(Collection collection)
        {
            this.col = collection;
        }

        private FileStream reader_new_stream(string nam) => new FileStream(nam, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 4096 * 4, FileOptions.SequentialScan);
        public IEnumerable<Row<T>> Create<T>(
             bool reverse, long StartPos = -1, Predicate<RowHeader> match = null, int Skip = 0,
             bool readValue = false, bool readDeleted = false, bool CheckState = true)
        {
            if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
            if (this.col.fh.Count == 0) yield break;
            if (StartPos == -1) StartPos = reverse ? this.col.cw.fs_inx.Length : FileHeader.Size;
            if (StartPos < FileHeader.Size) yield break;

            Func<byte[], T> CType = default;
            if (readValue)
                if (typeof(T) == typeof(byte[]) || typeof(T) == typeof(object))
                    CType = (bytes) => (T)(object)bytes;
                else
                    CType = (bytes) => Serializer.GetObject<T>(bytes);


            using (var fs_inx = reader_new_stream(this.col.IndexFile))
            using (var fs_dat = reader_new_stream(this.col.DataFile))
            {
                // correction for the start position
                StartPos = reader_posCorrection(fs_inx, StartPos);

                foreach (var rh in iterate(fs_inx, StartPos, reverse))
                {
                    if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
                    // macted rows is being returned with its value -if it was requested-
                    if ((readDeleted || rh.IsDeleted == false) &&
                        (match == null || match(rh)))
                    {
                        var retval = new Row<T>() { Header = rh };
                        if (readValue)
                        {
                            if (fs_dat.Position != rh.ValuePos) fs_dat.Position = rh.ValuePos;
                            var bytes = new byte[rh.ValueActualSize];
                            fs_dat.Read(bytes, 0, rh.ValueActualSize);

                            retval.Data = CType(bytes);
                        }

                        if (Skip > 0)
                        {
                            Skip--;
                            continue;
                        }
                        yield return retval;
                    }
                }
            }
        }

        public long Count(long StartPos = -1, Predicate<RowHeader> match = null, int Skip = 0, bool CheckState = true)
        {
            if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
            if (this.col.fh.Count == 0) return default;
            if (StartPos < FileHeader.Size) StartPos = FileHeader.Size;

            long count = 0;
            using (var fs_inx = reader_new_stream(this.col.IndexFile))
            {
                // correction for the start position
                StartPos = reader_posCorrection(fs_inx, StartPos);

                foreach (var rh in iterate(fs_inx, StartPos, false))
                {
                    if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
                    // macted rows is being returned with its value -if it was requested-
                    if (match == null || match(rh))
                    {
                        if (Skip > 0)
                        {
                            Skip--;
                            continue;
                        }
                        count++;
                    }
                }
            }
            return count;
        }

        private IEnumerable<RowHeader> iterate(FileStream fs_inx, long StartPos, bool Reverse)
        {
            fs_inx.Position = StartPos;
            var bufferLen = RowHeader.Size * 100;
            var bufferBytes = new byte[bufferLen];
            while (Reverse ? StartPos > FileHeader.Size : true)
            {
                { // BEFORE
                    if (Reverse)
                    {
                        // correction for the start position
                        StartPos -= bufferLen;
                        if (StartPos < FileHeader.Size)
                        {
                            bufferLen -= FileHeader.Size + ((int)StartPos * -1);
                            StartPos = FileHeader.Size;
                        }
                        // set the file position previous
                        fs_inx.Position = StartPos;
                    }
                }

                { // load and iterate buffer
                    var bufferCount = fs_inx.Read(bufferBytes, 0, bufferLen) / RowHeader.Size;
                    if (bufferCount == 0) yield break;

                    RowHeader getFromBuffer(int bufferPos) =>
                        reader_toRowHeader(bufferBytes, bufferPos, StartPos + bufferPos);

                    if (Reverse)
                        for (int i = bufferCount - 1; i >= 0; i--)
                            yield return getFromBuffer(i * 255);
                    else
                        for (int i = 0; i < bufferCount; i++)
                            yield return getFromBuffer(i * 255);

                }

                { // AFTER
                    if (Reverse == false)
                    {
                        StartPos += bufferLen;
                    }
                }
            }
        }

        private long reader_posCorrection(FileStream fs, long pos)
        {
            if
                (pos < FileHeader.Size) return FileHeader.Size;
            else if
                (pos > fs.Length) return fs.Length;

            return (long)(Math.Floor((decimal)(pos - FileHeader.Size) / RowHeader.Size) * RowHeader.Size) + FileHeader.Size;
        }

        private RowHeader reader_toRowHeader(byte[] bytes, int start, long pos)
        {
            var rh = new RowHeader();
            rh.FromArray(bytes, start);
            rh.Pos = pos;
            return rh;
        }
    }
}