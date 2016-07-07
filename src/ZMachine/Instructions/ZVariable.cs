using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Instructions
{
    public enum ZVariableLocation { Stack, Local, Global }

    public class ZVariable
    {
        /// <summary>
        /// the original bits of the variable type of the operand
        /// </summary>
        public ushort ID { get; set; }

        public ZVariableLocation Location
        {
            get
            {
                if (ID >= 16)
                {
                    return ZVariableLocation.Global;
                }
                else if (ID >= 1)
                {
                    return ZVariableLocation.Local;
                }
                else
                {
                    return ZVariableLocation.Stack;
                }
            }
        }

        public override string ToString()
        {
            switch (Location)
            {
                case ZVariableLocation.Global:
                    return "g" + ID.ToString();
                case ZVariableLocation.Local:
                    return "local" + ID.ToString();
                case ZVariableLocation.Stack:
                    return "sp";
                default:
                    throw new InvalidOperationException($"Unknown variable location '{Location}'");
            }
        }
    }
}