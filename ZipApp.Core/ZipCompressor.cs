using System;
using System.IO;
using ZipAppModel;

namespace ZipApp.Core
{
    public class ZipCompressor : ZipOperationChunk
    {
        private long _blockCount;
        public ZipCompressor()
        {           
            Operation = (block) => GZip.Compress(block);
        }
        public override void Reader(string source, ref SynchronizedQueue<Chunk> readerQueue)
        {
            try
            {
                FileInfo fi = new FileInfo(source);
                _blockCount = fi.Length / GZip.BUFFER_SIZE;
                if (fi.Length % GZip.BUFFER_SIZE > 0)
                {
                    _blockCount++;
                }
            }
            catch (Exception)
            {
                OnCancel();
                OnError();
                Console.WriteLine("Error read file");
                return;
            }

            try
            {
                using (BinaryReader br = new BinaryReader(new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None)))
                {
                    for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                    {
                        byte[] blockValue = br.ReadBytes(GZip.BUFFER_SIZE);

                        if (blockValue == null)
                        {
                            throw new ArgumentNullException("Error read file");
                        }
                        Chunk readerChunk = new Chunk(blockNumber, blockValue);
                        if (!readerQueue.TryEnqueue(readerChunk))
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnCancel();
                OnError();
                Console.WriteLine(e.Message);
                return;
            }
        }
        public override void Writer(string sink, ref SynchronizedQueue<Chunk> writerQueue)
        {
            int counter = 0;

            int blockNumber = -1;
            byte[] blockValue = null;

            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(sink, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    bw.Write(BitConverter.GetBytes(_blockCount));

                    while (true)
                    {
                        Chunk writerChunk = new Chunk(blockNumber, blockValue);
                        if (!writerQueue.TryDequeue(out writerChunk))
                        {
                            return;
                        }

                        if (writerChunk.Data == null)
                        {
                            break;
                        }

                        try
                        {
                            bw.Write(BitConverter.GetBytes(writerChunk.Id));
                            bw.Write(writerChunk.Data.Length);
                            bw.Write(writerChunk.Data);
                        }
                        catch (IOException)
                        {
                            OnCancel();
                            OnError();
                            Console.WriteLine("Error write compressing file");
                            bw.Close();
                            File.Delete(sink);
                            return;
                        }

                        counter++;

                        if (counter == _blockCount)
                        {
                            OnCancel();
                        }
                    }
                }
            }
            catch (Exception)
            {             
                OnCancel();
                OnError();
                Console.WriteLine("Error write compressing file");
            }
        }
    }
}