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
             bool reverse, long StartPos = -1, Predicate<RowHeader> match = null,
             bool readValue = false, bool readDeleted = false, bool CheckState = true)
        {
            if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
            if (this.col.fh.Count == 0) yield break;
            if (StartPos == -1) StartPos = reverse ? this.col.cw.fs_inx.Length : FileHeader.Size;
            if (StartPos < FileHeader.Size) yield break;

            using (var fs_inx = reader_new_stream(this.col.IndexFile))
            using (var fs_dat = reader_new_stream(this.col.DataFile))
            {
                // correction for the start position
                StartPos = reader_posCorrection(fs_inx, StartPos);

                foreach (var rh in reverse ? reader_backward(fs_inx, StartPos) : reader_forward(fs_inx, StartPos))
                {
                    if (CheckState) this.col.checkFileState(FileAccess.Read, FileState.Normal);
                    // macted rows is being returned with its value -if it was requested-
                    if ((readDeleted || rh.IsDeleted == false) &&
                        (match == null || match(rh)))
                    {
                        var retval = new Row<T>() { Header = rh };
                        if (readValue)
                            if (typeof(T) == typeof(byte[]) || typeof(T) == typeof(object))
                                retval.Data = (T)(object)reader_getValue(fs_dat, rh);
                            else
                                retval.Data = reader_getValue<T>(fs_dat, rh);
                        yield return retval;
                    }
                }
            }
        }

        private IEnumerable<RowHeader> reader_forward(FileStream fs_inx, long StartPos = -1)
        {
            fs_inx.Position = StartPos;
            var bufferLen = RowHeader.Size * 100;
            var bytes = new byte[bufferLen];
            while (true)
            {
                var lastIndex = fs_inx.Read(bytes, 0, bufferLen);
                if (lastIndex < RowHeader.Size) yield break;

                var startIndex = 0;
                while (startIndex < lastIndex)
                {
                    // row-header is being initialized from bytes read.
                    yield return reader_toRowHeader(bytes, startIndex, StartPos);

                    startIndex += RowHeader.Size;
                    StartPos += RowHeader.Size;
                }
            }
        }
        private IEnumerable<RowHeader> reader_backward(FileStream fs_inx, long StartPos = -1)
        {
            while (StartPos > FileHeader.Size)
            {
                // correction for the start position
                var bufferLen = RowHeader.Size * 100;
                StartPos -= bufferLen;
                if (StartPos < FileHeader.Size)
                {
                    bufferLen -= (FileHeader.Size - (int)StartPos);
                    StartPos = FileHeader.Size;
                }

                // buffer bytes is being read from filestream
                fs_inx.Position = StartPos;
                var bytes = new byte[bufferLen];
                fs_inx.Read(bytes);

                while (bufferLen > 0)
                {
                    bufferLen -= RowHeader.Size;

                    // row-header is being initialized from bytes read.
                    yield return reader_toRowHeader(bytes, bufferLen, StartPos + bufferLen);
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
        private byte[] reader_getValue(FileStream fs, RowHeader rh)
        {
            if (fs.Position != rh.ValuePos) fs.Position = rh.ValuePos;

            var bytes = new byte[rh.ValueActualSize];
            fs.Read(bytes);
            return bytes;
        }
        private T reader_getValue<T>(FileStream fs, RowHeader rh)
        {
            if (fs.Position != rh.ValuePos) fs.Position = rh.ValuePos;

            var bytes = new byte[rh.ValueActualSize];
            fs.Read(bytes, 0, rh.ValueActualSize);
            var retval = Serializer.GetObject<T>(bytes);
            return retval;
        }
    }
}