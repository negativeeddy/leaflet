using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Memory
{
    public class PackedAddress
    {
        public ushort Bits { get; }

        public PackedAddress(ushort bits)
        {
            Bits = bits;
        }

        int Address
        {
            get { return Bits * 2; }
        }
    }
}
