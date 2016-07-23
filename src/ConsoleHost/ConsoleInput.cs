using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Core.IO;

namespace ConsoleHost
{
    class ConsoleInput : IZInput
    {
        public char ReadChar()
        {
            return Console.ReadKey().KeyChar;
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
