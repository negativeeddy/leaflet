﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Memory
{
    public static class AddressHelper
    {
        public static uint UnpackAddress(ushort address)
        {
            return (uint)(address * 2); // unpack for v1-3
        }

        public static uint GetWordUnpacked(this IList<byte> data, int address)
        {
            ushort word = GetWord(data, address);
            return UnpackAddress(word);
        }

        public static ushort GetWord(this IList<byte> data, int address)
        {
            Debug.Assert(address <= data.Count);

            int upper = data[address] << 8;
            int lower = data[address + 1];
            uint uppershort = (uint)upper;
            uint lowershort = (uint)lower;
            return (ushort)(uppershort + lowershort);
        }

        public static void SetWord(this IList<byte> data, ushort word, int address)
        {
            Debug.Assert(address + 1 < data.Count);
            byte upper = (byte)(word >> 8);
            byte lower = (byte)word;
            data[address] = upper;
            data[address + 1] = lower;
        }

        public static IList<ushort> GetWords(this IList<byte> data, int address, int count)
        {
            Debug.Assert(address + count <= data.Count);

            ushort[] words = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                words[i] = GetWord(data, address + i * 2);
            }
            return words;
        }

        public static uint GetDWord(this IList<byte> data, int address)
        {
            return (uint)(data[address] << 24) +
                   (uint)(data[address + 1] << 16) +
                   (uint)(data[address + 2] << 8) +
                   (uint)(data[address + 3]);
        }

        /// <summary>
        /// Retrieves a subset of the bits in an 16-bit word
        /// </summary>
        /// <param name="word">the original 16-bit word</param>
        /// <param name="high">the highest bit to retrieve (LSB is 0, MSB is 15)</param>
        /// <param name="length">how many bits to retrieve</param>
        /// <returns></returns>
        public static ushort FetchBits(this ushort word, BitNumber high, int length)
        {
            var mask = ~(-1 << length);
            var result = (word >> ((int)high - length + 1)) & mask;
            return (ushort)result;
        }

        /// <summary>
        /// Retrieves a subset of the bits in an 8-bit byte
        /// </summary>
        /// <param name="word">the original 8-bit byte</param>
        /// <param name="high">the highest bit to retrieve (LSB is 0, MSB is 7)</param>
        /// <param name="length">how many bits to retrieve</param>
        /// <returns></returns>
        public static byte FetchBits(this byte theByte, BitNumber high, int length)
        {

            var mask = ~(-1 << length);
            var result = (theByte >> ((int)high - length + 1)) & mask;
            return (byte)result;
        }

        public static int ToWordZStringAddress(this ushort value)
        {
            return value * 2;
        }
    }

    /// <summary>
    /// The bit number used to retrieve bits via FetchBits(). This is not
    /// a bitflag, it is the ordinal position of the bit with the least 
    /// significant bit being 0
    /// </summary>
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