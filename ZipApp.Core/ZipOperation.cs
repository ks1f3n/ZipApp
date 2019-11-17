namespace ZipApp.Core
{  
    public abstract class ZipOperation<T>
    {
        public abstract void Reader(string source, ref SynchronizedQueue<T> readerQueue);
        public abstract void Process(ref SynchronizedQueue<T> readerTaskPool, ref SynchronizedQueue<T> writerQueue);
        public abstract void Writer(string sink, ref SynchronizedQueue<T> writerQueue);
    }
}