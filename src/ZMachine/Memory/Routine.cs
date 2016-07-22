using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public Stack<ushort> EvaluationStack { get; } = new Stack<ushort>();

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
        public Routine(IList<byte> bytes, int routineAddress, int returnAddress, ZVariable returnStore, IList<ushort> localInitValues)
        {
            Debug.Assert(routineAddress % 2 == 0, "A routine is required to begin at an address in memory which can be represented by a packed address (spec 5.1)");
            _bytes = bytes;
            _baseAddress = routineAddress;
            this.ReturnAddress = returnAddress;
            Store = returnStore;

            int localVariableCount = bytes[routineAddress];

            // initialize locals from the routine definition
            Locals = bytes.GetWords(routineAddress + 1, localVariableCount);

            // update locals with any provided arguments
            for (int i = 0; i < localInitValues.Count; i++)
            {
                if (i < localInitValues.Count)
                {
                    Locals[i] = localInitValues[i];
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Locals ");
            for (int i = 0; i < Locals.Count; i++)
            {
                sb.Append("local");
                sb.Append(i);
                sb.Append('=');
                sb.Append(Locals[i].ToString("x4"));
                sb.Append(' ');
            }
            sb.AppendLine();

            sb.Append("Stack");
            foreach (var item in EvaluationStack)
            {
                sb.Append(item.ToString());
                sb.Append(' ');
            }
            sb.AppendLine();

            sb.AppendLine($"Resume at: {ReturnAddress:x4}");

            return sb.ToString();
        }
    }
}