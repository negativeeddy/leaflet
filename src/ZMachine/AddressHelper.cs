using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Memory
{
    public static class AddressHelper
    {
        public static ushort GetWord(this IList<byte> data, int address)
        {
            int upper = data[address] << 8;
            int lower = data[address + 1];
            uint uppershort = (uint)upper;
            uint lowershort = (uint)lower;
            return (ushort)(uppershort + lowershort);
        }

        public static uint GetDWord(this IList<byte> data, int address)
        {
            return (uint)(data[address] << 24) +
                   (uint)(data[address + 1] << 16) +
                   (uint)(data[address + 2] << 8) +
                   (uint)(data[address + 3]);
        }

        public static ushort FetchBits(this ushort word, BitNumber high, int length)
        {
            var mask = ~(-1 << length);
            var result = (word >> ((int)high - length + 1)) & mask;
            return (ushort)result;
        }

        public static int ToWordZStringAddress(this ushort value)
        {
            return value * 2;
        } 
    }

    [Flags]
    public enum BitNumber : int
    {
        Bit_0 = 0,
        Bit_1,
        Bit_2,
        Bit_3,
        Bit_4,
        Bit_5,
        Bit_6,
        Bit_7,
        Bit_8,
        Bit_9,
        Bit_10,
        Bit_11,
        Bit_12,
        Bit_13,
        Bit_14,
        Bit_15,
    }
}