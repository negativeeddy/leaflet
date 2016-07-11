using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ZMachine.Instructions.Tests
{
    [TestClass]
    public class BranchOffsetTests
    {
        [TestMethod]
        public void BasicBranchOffsetTest()
        {
            var inputs = new Tuple<byte[], ushort, bool>[] {
                    new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("01010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("00010101",2), false),
                    new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("00010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("0001010111001110",2), false),
                    new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("11010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("00010101",2), true),
                    new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("10010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("0001010111001110",2), true),
                    new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("01011000", 2), Convert.ToByte("01010101", 2) }, 24, false),
           };

            foreach (var testData in inputs)
            {
                var offset = new BranchOffset(testData.Item1);
                bool actualOnTrue = offset.WhenTrue;
                bool expectedOnTrue = testData.Item3;
                Assert.AreEqual(expectedOnTrue, actualOnTrue, "Branch on true");
                ushort actualOffset = offset.Offset;
                ushort expectedOffset = testData.Item2;
                Assert.AreEqual(expectedOffset, actualOffset);
            }
        }
    }
}
