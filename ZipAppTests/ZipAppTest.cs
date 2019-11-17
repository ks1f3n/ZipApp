using FluentAssertions;
using System;
using System.IO;
using Xunit;
using ZipApp.CLI;

namespace ZipAppTests
{
    public class ZipAppTest
    {
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
    }
}
