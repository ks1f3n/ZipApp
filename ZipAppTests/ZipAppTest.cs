using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using ZipApp.CLI;
using ZipApp.Core;
using ZipAppModel;

namespace ZipAppTests
{
    public class ZipAppTest
    {
        private readonly string INPUT_FILE = "test_input.txt";
        private readonly string MID_OUTPUT_FILE = "test_output.txt";
        private readonly string OUTPUT_FILE = "test_input_res.txt";
        [Fact]
        public void ReadNotValidInputParamsTest()
        {
            string p1 = "compress";
            string p2 = "input.txt";
            string p3 = "input.gz";
            Action act = () => new ParseArgs(new string[] { p1, p2, p3 });
            act.Should().Throw<FileNotFoundException>()
                .WithMessage("Input file not found");
        }
        [Fact]
        public void CompressDecompressIntegratedTest()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[10];
            rnd.NextBytes(b);

            Compress(b);
            var res_b = Decompress();

            File.Delete(INPUT_FILE);
            File.Delete(MID_OUTPUT_FILE);
            File.Delete(OUTPUT_FILE);

            res_b.Should().NotBeEmpty();
            res_b.Length.Should().Be(10);
            res_b.Should().BeEquivalentTo(b);
        }
        public void Compress(Byte[] b)
        {
            File.WriteAllBytes(INPUT_FILE, b);

            var zipOperation = new ZipCompressor();
            var readerQueue = new SynchronizedQueue<Chunk>(4);
            var writerQueue = new SynchronizedQueue<Chunk>(4);

            zipOperation.Cancel += readerQueue.Close;
            zipOperation.Cancel += writerQueue.Close;

            var reader = new Thread(() => zipOperation.Reader(INPUT_FILE, ref readerQueue));
            var proces = new Thread(() => zipOperation.Process(ref readerQueue, ref writerQueue));
            var writer = new Thread(() => zipOperation.Writer(MID_OUTPUT_FILE, ref writerQueue));

            reader.Start();
            proces.Start();
            writer.Start();

            reader.Join();
            proces.Join();
            writer.Join();

            zipOperation.Cancel -= readerQueue.Close;
            zipOperation.Cancel -= writerQueue.Close;

        }
        public Byte[] Decompress()
        {
            var zipOperation = new ZipDecompressor();
            var readerQueue = new SynchronizedQueue<Chunk>(4);
            var writerQueue = new SynchronizedQueue<Chunk>(4);

            zipOperation.Cancel += readerQueue.Close;
            zipOperation.Cancel += writerQueue.Close;

            var reader = new Thread(() => zipOperation.Reader(MID_OUTPUT_FILE, ref readerQueue));
            var proces = new Thread(() => zipOperation.Process(ref readerQueue, ref writerQueue));
            var writer = new Thread(() => zipOperation.Writer(OUTPUT_FILE, ref writerQueue));

            reader.Start();
            proces.Start();
            writer.Start();

            reader.Join();
            proces.Join();
            writer.Join();

            zipOperation.Cancel -= readerQueue.Close;
            zipOperation.Cancel -= writerQueue.Close;

            return File.ReadAllBytes("test_input_res.txt");
        }
    }
}
