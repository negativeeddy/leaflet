using NegativeEddy.Leaflet.IO;

namespace NegativeEddy.Leaflet.CoreConsoleHost;

class ConsoleInput : IZInput
{
    public char ReadChar() => Console.ReadKey().KeyChar;

    public string? ReadLine() => Console.ReadLine();
}
