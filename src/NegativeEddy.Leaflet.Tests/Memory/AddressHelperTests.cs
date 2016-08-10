using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NegativeEddy.Leaflet.Memory.Tests
{
    [TestClass()]
    public class AddressHelperTests
    {
        [TestMethod]
        public void SetBitTest()
        {
            var inputs = new Tuple<uint, BitNumber, uint>[]
            {
                new Tuple<uint,BitNumber,uint>(0x0000u, BitNumber.Bit_0,  0x00000001u),
                new Tuple<uint,BitNumber,uint>(0x0000u, BitNumber.Bit_31, 0x80000000u),
                new Tuple<uint,BitNumber,uint>(0x0000u, BitNumber.Bit_5,  0x00000020u),
            };

            foreach(var data in inputs)
            {
                // set  bits
                uint dword = data.Item1;
                uint actual = dword.SetBit(data.Item2, true);
                uint expected = data.Item3;
                Assert.AreEqual(expected, actual);

                // clear bits
                dword = ~data.Item1;
                actual = dword.SetBit(data.Item2, false);
                expected = ~data.Item3;
                Assert.AreEqual(expected, actual);
            }


        }

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
        public void SetDWordTest()
        {
            var inputs = new List<Tuple<byte[], uint>>()
            {
                //  expected result, test index, test value
                new Tuple<byte[], uint>(new byte[] { 0x01, 0x23, 0x34, 0x56 }, 0x01233456),
                new Tuple<byte[], uint>(new byte[] { 0x23, 0x34, 0x56, 0x87 },  0x23345687),
                new Tuple<byte[], uint>(new byte[] { 0x34, 0x56, 0x87, 0x9F },  0x3456879F),
            };

            byte[] starterBytes = Enumerable.Range(0, 20).Select(x => (byte)0xff).ToArray();
            Console.WriteLine("Initial data");
            Console.WriteLine(ArrayToString(starterBytes));
            Console.WriteLine();

            for (int i = 0; i < starterBytes.Length - 3; i++)
            {
                // run the indices from the beginning to the end of the array
                foreach (var input in inputs)
                {
                    byte[] testData = (byte[])starterBytes.Clone();
                    testData.SetDWord(input.Item2, i);

                    Console.WriteLine(ArrayToString(testData));

                    byte[] expectedResults = input.Item1;
                    for(int testIdx = 0; testIdx < expectedResults.Length; testIdx++)
                    {
                        Assert.AreEqual(expectedResults[testIdx], testData[testIdx + i], $"i={i}, testIdx={testIdx}");
                    }
                }
            }
        }

        private string ArrayToString(IList<byte> bytes)
        {
            return bytes.Aggregate(string.Empty, (current, b) => current += $"0x{b:x2},", x => x);
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