using System;
using System.Reactive;

namespace NegativeEddy.Leaflet.IO
{
    public interface IZInput
    {
        string ReadLine();
        char ReadChar();
    }
}
