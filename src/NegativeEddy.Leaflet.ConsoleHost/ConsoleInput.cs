using System;
using NegativeEddy.Leaflet.IO;

namespace NegativeEddy.Leaflet.ConsoleHost
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
