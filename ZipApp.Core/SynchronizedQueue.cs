using System.Collections.Generic;
using System.Threading;

namespace ZipApp.Core
{
    /// <summary>
    /// Concurrent blocking queue with predefined size    
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronizedQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private readonly int maxSize;
        private bool closing;
        public SynchronizedQueue(int maxSize) { this.maxSize = maxSize; }
        public void Close()
        {
            lock (queue)
            {
                closing = true;
                Monitor.PulseAll(queue);
            }
        }
        public bool TryEnqueue(T value)
        {
            lock (queue)
            {
                while (queue.Count >= maxSize)
                {
                    if (closing)
                    {
                        return false;
                    }
                    Monitor.Wait(queue);
                }
                queue.Enqueue(value);
                if (queue.Count == 1)
                {
                    //wake up any blocked dequeue
                    Monitor.PulseAll(queue);
                }
                return true;
            }
        }
        public bool TryDequeue(out T value)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closing)
                    {
                        value = default(T);
                        return false;
                    }
                    Monitor.Wait(queue);
                }
                value = queue.Dequeue();
                if (queue.Count == maxSize - 1)
                {
                    // wake up any blocked enqueue
                    Monitor.PulseAll(queue);
                }
                return true;
            }
        }
    }
}
