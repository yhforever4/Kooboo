﻿using System; 
using System.IO;
 
namespace Kooboo.IndexedDB.Dynamic
{
  
    public class BlockFile
    {

        private string _fullfilename;
        private FileStream _filestream;

        private object _object = new object();

        public BlockFile(string fullfilename)
        {
            this._fullfilename = fullfilename;
        }

        public void OpenOrCreate()
        {
            if (!File.Exists(this._fullfilename))
            {
                // file not exists.first check directory exists or not.
                string dirname = Path.GetDirectoryName(this._fullfilename);
                if (!System.IO.Directory.Exists(dirname))
                {
                    System.IO.Directory.CreateDirectory(dirname);
                }

                File.WriteAllText(this._fullfilename, "block content file, do not modify\r\n");
            }
        }

        private byte[] GetPartial(long position, int offset, int count)
        {
            byte[] partial = new byte[count];
            Stream.Position = position + offset;
            Stream.Read(partial, 0, count);
            return partial;
        }

        // keep for upgrade.. not used any more. 
        public byte[] GetContent(long position, int KeyColumnOffset)
        {
            byte[] counterbytes = GetPartial(position, 26, 4);
            int counter = BitConverter.ToInt32(counterbytes, 0);
            return GetPartial(position, 30 + KeyColumnOffset, counter);
        }

        public byte[] GetKey(long position, int ColumnOffset, int KeyLength)
        {
            return GetPartial(position, 30 + ColumnOffset, KeyLength);
        }

        #region  NewAPI

        public long Add(byte[] bytes, int TotalByteLen)
        {
            byte[] header = new byte[10];
            header[0] = 10;
            header[1] = 13;
            System.Buffer.BlockCopy(BitConverter.GetBytes(TotalByteLen), 0, header, 2, 4); 

            int tolerance =  TotalByteLen * 2; 
            System.Buffer.BlockCopy(BitConverter.GetBytes(tolerance), 0, header, 6, 4);

            byte[] total = new byte[tolerance];

            System.Buffer.BlockCopy(bytes, 0,  total, 0, TotalByteLen);

            Int64 currentposition;
            currentposition = Stream.Length;
            Stream.Position = currentposition;
            Stream.Write(header, 0, 10);
            Stream.Write(total, 0, tolerance);
              
            return currentposition;  
        }

        public void UpdateBlock(byte[] bytes, long blockposition)
        {
            byte[] counter = BitConverter.GetBytes(bytes.Length);

            Stream.Position = blockposition + 2;

            Stream.Write(counter, 0, 4);

           // Stream.Position = blockposition + 6;  

            Stream.Position = blockposition + 10;
            Stream.Write(bytes, 0, bytes.Length);  
             
        }


        public byte[] Get(long position)
        {
            byte[] counterbytes = GetPartial(position, 2, 4);
            int counter = BitConverter.ToInt32(counterbytes, 0);
            return GetPartial(position, 10, counter);
        }

        public int GetTolerance(long position)
        {
            byte[] counterbytes = GetPartial(position, 6, 4);
            return  BitConverter.ToInt32(counterbytes, 0); 
        }



        public byte[] Get(long position, int ColumnLen)
        {
            return GetPartial(position, 10, ColumnLen);
        }

        public byte[] GetCol(long position, int relativePos, int len)
        {
            if (len > 0)
            {
                return GetPartial(position, relativePos + 10 + 8, len);
            }
            else
            {
                // TODO: This should not needed.... 
            }
            return null;
        }

        public void UpdateCol(long position, int relativeposition, int length, byte[] values)
        {
            this.Stream.Position = position + 10 + relativeposition + 8;
            this.Stream.Write(values, 0, length);
        }

        #endregion
        public void Close()
        {
            if (_filestream != null)
            {
                lock (_object)
                {
                    if (_filestream != null)
                    {
                        _filestream.Close();
                        _filestream = null;
                    }
                }
            }
        }

        public void Flush()
        {
            if (_filestream != null)
            {
                lock (_object)
                {
                    _filestream.Flush();
                }
            }
        }

        public FileStream Stream
        {

            get
            {

                if (_filestream == null || !_filestream.CanRead)
                {
                    lock (_object)
                    {
                        if (_filestream == null || !_filestream.CanRead)
                        {
                            this.OpenOrCreate();
                            _filestream = StreamManager.GetFileStream(this._fullfilename);
                        }
                    }
                }
                return _filestream;
            }
        }

    }
      
}