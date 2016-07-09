using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZMachine.Memory;

namespace ZMachine
{
    public class Routine
    {
        private IList<byte> _bytes;
        private int _baseAddress;

        public int FirstInstructionAddress
        {
            // first instruction starts after the local count, then after the local words.
            get { return _baseAddress + 1 + Locals.Count * 2; }
        }
        public IList<ushort> Locals { get; }

        public Routine(IList<byte> bytes, int routineAddress)
        {
            Debug.Assert(routineAddress % 2 == 0, "A routine is required to begin at an address in memory which can be represented by a packed address (spec 5.1)");
            _bytes = bytes;
            _baseAddress = routineAddress;

            int count = bytes[routineAddress];
            Locals = bytes.GetWords(routineAddress + 1, count);
        }
    }
}