using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Instructions
{
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
            string operandCount = specIdentifier.Substring(0, underscoreIdx);
            OperandCount = (OperandCountType)Enum.Parse(typeof(OperandCountType), operandCount);

            OpcodeNumber = ushort.Parse(specIdentifier.Substring(underscoreIdx + 1));
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

        public override bool Equals(object obj)
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
        private ushort NameToOpcodeDifference()
        {
            switch (OperandCount)
            {
                case OperandCountType.OP2:
                case OperandCountType.EXT:
                    return 0;
                case OperandCountType.OP1:
                    return 128;
                case OperandCountType.OP0:
                    return 176;
                case OperandCountType.VAR:
                    return 224;
                default:
                    throw new InvalidOperationException();
            }
        }

        static public int GetOpcodeIdentifier(OperandCountType operandCount, ushort opcodeNumber)
        {
            return (ushort)operandCount << 16 | opcodeNumber;
        }
    }
}
