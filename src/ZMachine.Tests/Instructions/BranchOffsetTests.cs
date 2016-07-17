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

            var inputs = new Tuple<byte[], int, bool>[] {    // test bytes, expected Offset, expected BranchOnTrue 
                    //new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("01010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("00010101",2), false),
                    //new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("00010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("0001010111001110",2), false),
                    //new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("11010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("00010101",2), true),
                    //new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("10010101", 2), Convert.ToByte("11001110", 2) }, Convert.ToUInt16("0001010111001110",2), true),
                    //new Tuple<byte[], ushort, bool>( new byte[] { Convert.ToByte("01011000", 2), Convert.ToByte("01010101", 2) }, 24, false),

                    // expected offset is JumpAddress - CurrentAddress + NumBytesInInstruction + 2
                    new Tuple<byte[], int, bool>(new byte[] {0x48, 0x00 }, 0x6d79 - (0x6d6f + 4) + 2, false),   // minizork:  6d6f:  61 04 07 48     JE  L03,L06 [FALSE] 6d79
                    new Tuple<byte[], int, bool>(new byte[] {0x80, 0x41 }, 0x6dbd - (0x6d79 + 5) + 2, true), // minizork:  6d79:  61 04 83 80 41  JE  L03,G73 [TRUE]  6dbd
                    new Tuple<byte[], int, bool>(new byte[] {0xbf, 0xdb }, 0x6d48 - (0x6d6b + 4) + 2, true), // minizork:  6d6b:  a0 04 bf db             JZ L03[TRUE] 6d48};
                    };

            foreach (var testData in inputs)
            {
                var offset = new BranchOffset(testData.Item1);
                bool actualOnTrue = offset.WhenTrue;
                bool expectedOnTrue = testData.Item3;
                Assert.AreEqual(expectedOnTrue, actualOnTrue, $"Failed for bytes [{testData.Item1[0]:x} {testData.Item1[1]:x}]");
                int actualOffset = offset.Offset;
                int expectedOffset = testData.Item2;
                
                Assert.AreEqual(expectedOffset, actualOffset, $"Failed for bytes [{testData.Item1[0]:x} {testData.Item1[1]:x}]");
            }
        }
    }
}
