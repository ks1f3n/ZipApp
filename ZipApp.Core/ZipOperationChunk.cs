using System;
using ZipAppModel;

namespace ZipApp.Core
{
    public delegate void CancelEventHandler();
    public delegate void ErrorResultEventHandler();
    public abstract class ZipOperationChunk : ZipOperation<Chunk>
    {
        public event CancelEventHandler Cancel;
        public event ErrorResultEventHandler Error;
        public Func<byte[], byte[]> Operation { get; set; }
        protected void OnCancel()
        {
            Cancel();            
        }
        protected void OnError()
        {            
            Error();
        }
        public override void Process(ref SynchronizedQueue<Chunk> readerQueue, ref SynchronizedQueue<Chunk> writerQueue)
        {
            int blockNumber = -1;
            byte[] blockValue = null;

            while (true)
            {
                try
                {
                    Chunk readerChunk = new Chunk(blockNumber, blockValue);
                    if (!readerQueue.TryDequeue(out readerChunk))
                    {
                        return;
                    }

                    if (readerChunk.Data == null)
                    {
                        break;
                    }

                    byte[] blockData = Operation(readerChunk.Data);      

                    Chunk writerChunk = new Chunk(readerChunk.Id, blockData);
                    if (!writerQueue.TryEnqueue(writerChunk))
                    {
                        return;
                    }
                }
                catch (Exception)
                {                    
                    Cancel();
                    Error();
                    Console.WriteLine("Error compressing");
                    return;
                }
            }
        }
    }
}
