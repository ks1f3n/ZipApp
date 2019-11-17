using System;
using System.Threading;

namespace ZipApp.CLI
{
    internal class Result
    {
        private int m_errorReported = 0;
        public void Ok()
        {
            if (m_errorReported == 0)
            {
                Console.WriteLine(1);
            }            
        }
        public void Error()
        {
            if (Interlocked.Exchange(ref m_errorReported, 1) == 0)
            {
                Console.WriteLine(0);
            }
        }
    }
}
