using System;
using System.Collections.Generic;
using System.IO;
using ZipAppModel;

namespace ZipApp.Core
{
    public class ZipDecompressor : ZipOperationChunk
    {        
        private long _blockCount;
        public ZipDecompressor()
        {
            Operation = (block) => GZip.Decompress(block);
        }
        public override void Reader(string source, ref SynchronizedQueue<Chunk> readerQueue)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None)))
                {
                    _blockCount = br.ReadInt64();

                    for (int count = 0; count < _blockCount; count++)
                    {
                        int blockNumber = br.ReadInt32();
                        int blockLength = br.ReadInt32();
                        byte[] blockValue = br.ReadBytes(blockLength);

                        if (blockValue == null)
                        {
                            throw new ArgumentNullException("Error read compressing file");
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
            int cnt = 0;

            int blockNumber = -1;
            byte[] blockValue = null;

            Dictionary<int, byte[]> buffer = new Dictionary<int, byte[]>();

            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(sink, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
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

                        buffer[writerChunk.Id] = writerChunk.Data;

                        while (buffer.ContainsKey(cnt))
                        {
                            bw.Write(buffer[cnt]);
                            buffer.Remove(cnt);

                            cnt++;

                            if (cnt == _blockCount)
                            {
                                OnCancel();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                OnCancel();
                OnError();
                Console.WriteLine("Error write decompressing file");
            }
        }
    }
}
