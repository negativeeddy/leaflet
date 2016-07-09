using System.Collections.Generic;
using System.Diagnostics;
using ZMachine.Instructions;

namespace ZMachine.Memory
{
    /// <summary>
    /// Represents a single routine and its current state (evaluation stack, 
    /// return address, local variables, etc)
    /// </summary>
    public class Routine
    {
        private readonly IList<byte> _bytes;
        private readonly int _baseAddress;

        public int ReturnAddress { get; } = -1;

        /// <summary>
        /// The Store of the call instruction. The Routine will put its return value
        /// in this location
        /// </summary>
        public ZVariable Store { get; }
        public IList<ushort> Locals { get; }

        public int FirstInstructionAddress
        {
            // first instruction starts after the local count, then after the local words.
            get { return _baseAddress + 1 + Locals.Count * 2; }
        }

        /// <summary>
        /// Constructs a new Routine object from the address in
        /// a byte array
        /// </summary>
        /// <param name="bytes">bytes representing memory</param>
        /// <param name="routineAddress">the beginning of the Routine's frame in memory</param>
        public Routine(IList<byte> bytes, int routineAddress)
        {
            Debug.Assert(routineAddress % 2 == 0, "A routine is required to begin at an address in memory which can be represented by a packed address (spec 5.1)");
            _bytes = bytes;
            _baseAddress = routineAddress;

            int count = bytes[routineAddress];
            Locals = bytes.GetWords(routineAddress + 1, count);
        }

        public Stack<ushort> EvaluationStack = new Stack<ushort>();
    }
}