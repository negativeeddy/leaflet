using System;
using NegativeEddy.Leaflet.IO;

namespace NegativeEddy.Leaflet.ConsoleHost
{
    class ConsoleInput : IZInput
    {
        public char ReadChar() => Console.ReadKey().KeyChar;

        public string ReadLine() => Console.ReadLine();
    }
}
