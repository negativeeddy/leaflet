using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
            var inputs = new List<Tuple<byte[], int, uint>>()
            {
                // data, test index, expected result
                new Tuple<byte[], int, uint>(new byte[] { 0x01, 0x23, 0x34, 0x56, 0x87, 0x9F }, 0, 0x01233456),
                new Tuple<byte[], int, uint>(new byte[] { 0x01, 0x23, 0x34, 0x56, 0x87, 0x9F }, 1, 0x23345687),
                new Tuple<byte[], int, uint>(new byte[] { 0x01, 0x23, 0x34, 0x56, 0x87, 0x9F }, 2, 0x3456879F),
            };

            foreach (var input in inputs)
            {
                byte[] data = input.Item1;
                int address = input.Item2;

                uint expected = input.Item3;
                uint actual = data.GetDWord(address);
                Assert.AreEqual(expected, actual, input.ToString());
            }
        }

        [TestMethod()]
        public void TestWordFetchBits()
        {
            ushort i = Convert.ToUInt16("1011111101101100", 2);

            ushort expected = Convert.ToUInt16("101", 2);
            ushort actual = i.FetchBits(BitNumber.Bit_5, 3);
            Assert.AreEqual(expected, actual, "item 1");

            expected = Convert.ToUInt16("10", 2);
            actual = i.FetchBits(BitNumber.Bit_2, 2);
            Assert.AreEqual(expected, actual, "item 2");

            expected = Convert.ToUInt16("11", 2);
            actual = i.FetchBits(BitNumber.Bit_3, 2);
            Assert.AreEqual(expected, actual, "item 3");

            // MSB
            expected = Convert.ToUInt16("1011", 2);
            actual = i.FetchBits(BitNumber.Bit_15, 4);
            Assert.AreEqual(expected, actual, "MSB");

            // LSB
            expected = Convert.ToUInt16("1100", 2);
            actual = i.FetchBits(BitNumber.Bit_3, 4);
            Assert.AreEqual(expected, actual, "LSB");

        }

        [TestMethod()]
        public void TestByteFetchBits()
        {

            byte b = Convert.ToByte("10110111", 2);

            byte expected = Convert.ToByte("110", 2);
            ushort actual = b.FetchBits(BitNumber.Bit_5, 3);
            Assert.AreEqual(expected, actual, "item 1");

            expected = Convert.ToByte("11", 2);
            actual = b.FetchBits(BitNumber.Bit_2, 2);
            Assert.AreEqual(expected, actual, "item 1");

            expected = Convert.ToByte("01", 2);
            actual = b.FetchBits(BitNumber.Bit_3, 2);
            Assert.AreEqual(expected, actual, "item 1");

            // MSB
            expected = Convert.ToByte("1011", 2);
            actual = b.FetchBits(BitNumber.Bit_7, 4);
            Assert.AreEqual(expected, actual, "MSB");

            // LSB
            expected = Convert.ToByte("0111", 2);
            actual = b.FetchBits(BitNumber.Bit_3, 4);
            Assert.AreEqual(expected, actual, "LSB");
        }
    }
}