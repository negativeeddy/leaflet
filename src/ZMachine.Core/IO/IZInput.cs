using System;
using System.Reactive;

namespace ZMachine.Core.IO
{
    public interface IZInput
    {
        string ReadLine();
        char ReadChar();
    }
}
