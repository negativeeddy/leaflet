using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Instructions
{
    public enum OperandTypes
    {
        LargeConstant,
        SmallConstant,
        Variable,
        Omitted
    };
}
