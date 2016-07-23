using System;
using System.Collections.Generic;
using ZMachine.Core.IO;

namespace ZMachine.Tests.TestHelpers
{
    class InputFeeder : IZInput
    {
        private Queue<string> _input;
        private bool _writeToConsole;

        public InputFeeder(IEnumerable<string> input, bool writeToConsole = true)
        {
            _input = new Queue<string>(input);
            _writeToConsole = writeToConsole;
        }

        public char ReadChar()
        {
            throw new NotImplementedException();
        }

        public string ReadLine()
        {
            string nextInput = _input.Dequeue();
            if (_writeToConsole)
            {
                Console.WriteLine(nextInput);
            }
            return nextInput;
        }
    }
}
