using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Sanatana.Timers
{
    public class PersistFile
    {
        //fields
        protected ReaderWriterLockSlim _fileLocker;
        protected FileInfo _fileInfo;


        //init
        public PersistFile(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _fileLocker = new ReaderWriterLockSlim();
        }
        

        //methods
        protected virtual FileInfo GetFile(bool createDirectory)
        {
            _fileInfo.Directory.Refresh();
            bool directoryCreated = _fileInfo.Directory.Exists;

            if (!directoryCreated && createDirectory)
            {
                _fileInfo.Directory.Create();
            }

            _fileInfo.Refresh();
            return _fileInfo;
        }

        public virtual void WriteLine(string line)
        {
            try
            {
                _fileLocker.EnterWriteLock();

                FileInfo fileInfo = GetFile(true);
                if (fileInfo.Exists)
                    fileInfo.Delete();

                using (FileStream fileStream = fileInfo.OpenWrite())
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(line);
                }
            }
            finally
            {
                _fileLocker.ExitWriteLock();
            }
        }

        public virtual string ReadLine()
        {
            string line = null;
          
            try
            {
                _fileLocker.EnterReadLock();

                FileInfo fileInfo = GetFile(false);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    return line;
                
                using (FileStream fileStream = fileInfo.OpenRead())
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        line = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                    }
                }
            }
            finally
            {
                _fileLocker.ExitReadLock();
            }

            return line;
        }
    }
}
