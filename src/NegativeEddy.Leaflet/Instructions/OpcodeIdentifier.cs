using System;

namespace NegativeEddy.Leaflet.Instructions
{
    /// <summary>
    /// This class provides a unique identifier for an opcode made up
    /// of the operand count and the opcode number
    /// </summary>
    public class OpcodeIdentifier
    {
        public OperandCountType OperandCount;
        public ushort OpcodeNumber;

        public OpcodeIdentifier(OperandCountType operandCount, ushort opcodeNumber)
        {
            OperandCount = operandCount;
            OpcodeNumber = opcodeNumber;
        }

        public OpcodeIdentifier(string specIdentifier)
        {
            int underscoreIdx = specIdentifier.IndexOf('_');
            string operandCount = specIdentifier[0..underscoreIdx];
            OperandCount = (OperandCountType)Enum.Parse(typeof(OperandCountType), operandCount);

            OpcodeNumber = ushort.Parse(specIdentifier[(underscoreIdx + 1)..]);
            OpcodeNumber -= NameToOpcodeDifference();
        }

        public int ID
        {
            get { return GetOpcodeIdentifier(OperandCount, OpcodeNumber); }
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return ((OpcodeIdentifier)obj).ID == ID;
        }

        public override string ToString()
        {
            return $"{OperandCount}_{OpcodeNumber + NameToOpcodeDifference()}";
        }

        /// <summary>
        /// This returns the difference between the number in the name of the opcode
        /// and the actual opcode value. e.g. VAR_226 is opcode 0x02, 226-2 = 224. 
        /// </summary>
        /// <returns></returns>
        private ushort NameToOpcodeDifference() => OperandCount switch
        {
            OperandCountType.OP2 or OperandCountType.EXT => 0,
            OperandCountType.OP1 => 128,
            OperandCountType.OP0 => 176,
            OperandCountType.VAR => 224,
            _ => throw new InvalidOperationException(),
        };

        static public int GetOpcodeIdentifier(OperandCountType operandCount, ushort opcodeNumber)
        {
            return (ushort)operandCount << 16 | opcodeNumber;
        }
    }
}
