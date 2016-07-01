using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZMachine.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Memory.Tests
{
    [TestClass()]
    public class AddressHelperTests
    {
        [TestMethod()]
        public void GetWordTest()
        {
            var inputs = new List<Tuple<byte[], int, ushort>>()
            {
                new Tuple<byte[], int, ushort>(new byte[] { 0x01, 0x02, 0x00 }, 0, 0x0102),
                new Tuple<byte[], int, ushort>(new byte[] { 0x02, 0x01, 0x00 }, 0, 0x0201),
                new Tuple<byte[], int, ushort>(new byte[] { 0x34, 0x56, 0x00 }, 0, 0x3456),
                new Tuple<byte[], int, ushort>(new byte[] { 0x77, 0x77, 0x00 }, 0, 0x7777),
                new Tuple<byte[], int, ushort>(new byte[] { 0x89, 0x89, 0x00 }, 0, 0x8989),
                new Tuple<byte[], int, ushort>(new byte[] { 0x89, 0x98, 0x00 }, 0, 0x8998),

                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x01, 0x02 }, 1, 0x0102),
                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x02, 0x01 }, 1, 0x0201),
                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x34, 0x56 }, 1, 0x3456),
                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x77, 0x77 }, 1, 0x7777),
                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x89, 0x89 }, 1, 0x8989),
                new Tuple<byte[], int, ushort>(new byte[] { 0x00, 0x89, 0x98 }, 1, 0x8998),
};

            foreach (var input in inputs)
            {
                byte[] data = input.Item1;
                int address = input.Item2;

                ushort expected = input.Item3;
                ushort actual = data.GetWord(address);
                Assert.AreEqual(expected, actual, input.ToString());
            }
        }

        [TestMethod()]
        public void GetDWordTest()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void TestFetchBits()
        {
            ushort i = Convert.ToUInt16("01101100", 2);

            ushort expected = Convert.ToUInt16("101", 2);
            ushort actual = i.FetchBits(BitNumber.Bit_5, 3);
            Assert.AreEqual(expected, actual);

            expected = Convert.ToUInt16("00", 2);
            actual = i.FetchBits(BitNumber.Bit_0, 2);
            Assert.AreEqual(expected, actual);

            expected = Convert.ToUInt16("11", 2);
            actual = i.FetchBits(BitNumber.Bit_3, 2);
            Assert.AreEqual(expected, actual);

            // MSB
            expected = Convert.ToUInt16("0110", 2);
            actual = i.FetchBits(BitNumber.Bit_7, 4);
            Assert.AreEqual(expected, actual);

            // LSB
            expected = Convert.ToUInt16("1100", 2);
            actual = i.FetchBits(BitNumber.Bit_3, 4);
            Assert.AreEqual(expected, actual);
        }
    }
}