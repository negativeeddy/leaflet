using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Instructions
{
    /// <summary>
    /// Operands reference either a Constant value or a Variable
    /// </summary>
    public class ZOperand
    {
        public OperandTypes Type { get; set; }
        public ZVariable Variable { get; set; }
        public uint Constant { get; set; }

        public int LengthInBytes
        {
            get
            {
                switch (Type)
                {
                    case OperandTypes.LargeConstant:
                        return 2;
                    case OperandTypes.SmallConstant:
                    case OperandTypes.Variable:
                        return 1;
                    case OperandTypes.Omitted:
                        return 0;
                    default:
                        throw new InvalidOperationException("Unknown Operand Type");
                }
            }
        }
    }
}
