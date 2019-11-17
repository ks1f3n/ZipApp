using System;
using System.Threading;
using ZipApp.Core;
using ZipAppModel;

namespace ZipApp.CLI
{
    class Program
    {
        private static int _cores = Environment.ProcessorCount;
        private static SynchronizedQueue<Chunk> _readerQueue;
        private static SynchronizedQueue<Chunk> _writerQueue;
        private static Thread _reader;
        private static Thread[] _process = new Thread[_cores];
        private static Thread _writer;
        static ZipOperationChunk ZipOperation = null;

        static void Main(string[] args)
        {
            ParseArgs options;
            Result result = new Result();
            try
            {
                options = new ParseArgs(args);
            }
            catch (Exception e)
            {
                result.Error();
                Console.WriteLine(e.Message);
                return;
            }

            if (options.Command == Operation.Compress)
            {
                ZipOperation = new ZipCompressor();
            }
            else
            {
                ZipOperation = new ZipDecompressor();
            }

            _readerQueue = new SynchronizedQueue<Chunk>(_cores);
            _writerQueue = new SynchronizedQueue<Chunk>(_cores);

            ZipOperation.Cancel += _readerQueue.Close;
            ZipOperation.Cancel += _writerQueue.Close;
            ZipOperation.Error += result.Error;

            _reader = new Thread(() => ZipOperation.Reader(options.Source, ref _readerQueue));

            for (int i = 0; i < _cores; i++)
            {
                _process[i] = new Thread(() => ZipOperation.Process(ref _readerQueue, ref _writerQueue));
            }

            _writer = new Thread(() => ZipOperation.Writer(options.Sink, ref _writerQueue));

            _reader.Start();
            foreach (Thread proces in _process)
            {
                proces.Start();
            }
            _writer.Start();

            _reader.Join();
            foreach (Thread proces in _process)
            {
                proces.Join();
            }
            _writer.Join();

            result.Ok();

            ZipOperation.Cancel -= _writerQueue.Close;
            ZipOperation.Cancel -= _readerQueue.Close;
            ZipOperation.Error -= result.Error;
        }
    }
}