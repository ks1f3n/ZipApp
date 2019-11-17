using System;
using System.IO;
using ZipAppModel;

namespace ZipApp.CLI
{
    public class ParseArgs
    {
        public Operation Command;

        public readonly string Source;

        public readonly string Sink;

        public ParseArgs(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Require only 3 parameters", "args");
            }
            switch (args[0])
            {
                case "compress":
                    Command = Operation.Compress;
                    break;
                case "decompress":
                    Command = Operation.Decompress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("args[0]", "'compress | decompress'", "first argument must be");
            }
            if (!File.Exists(args[1]))
            {
                throw new FileNotFoundException("Input file not found", args[1]);
            }
            Source = args[1];
            if (!Directory.Exists(Path.GetDirectoryName(args[2])))
            {
                throw new DirectoryNotFoundException("Output directory not found");
            }
            Sink = args[2];
        }
    }
}
