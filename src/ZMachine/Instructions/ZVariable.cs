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
        public ushort Data { get; set; }

        public ZVariableLocation Location
        {
            get
            {
                if (Value >= 16)
                {
                    return ZVariableLocation.Global;
                }
                else if (Value >= 1)
                {
                    return ZVariableLocation.Local;
                }
                else
                {
                    return ZVariableLocation.Stack;
                }
            }
        }

        public ushort Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}