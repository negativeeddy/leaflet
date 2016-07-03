using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ZMachine.Tests
{
    [TestClass]
    public class FileHeaderTests
    {
        [TestMethod]
        public void VerifyMiniZorkHeader()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            Assert.AreEqual(3, zm.MainMemory.Header.Version);
            //Assert.AreEqual(53248, zm.MainMemory.Header.Filelength);
            Assert.AreEqual(34, zm.MainMemory.Header.ReleaseNumber);
            Assert.AreEqual("871124", zm.MainMemory.Header.SerialCode);
            Assert.AreEqual(0x2187, zm.MainMemory.Header.StaticMemoryAddress);
            Assert.AreEqual(0x3709, zm.MainMemory.Header.HighMemoryAddress);
        }

        [TestMethod]
        public void VerifyAbbreviationTable()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var abbreviations = zm.MainMemory.AbbreviationTable().ToArray();
            foreach (ushort addr in abbreviations)
            {
                string data = zm.MainMemory.ReadString(addr);
                Console.WriteLine(data);
            }

            string expected = "The ";
            string actual = zm.MainMemory.ReadString(abbreviations[1]);
            Assert.AreEqual(expected, actual);

            expected = "It\'s ";
            actual = zm.MainMemory.ReadString(abbreviations[23]);
            Assert.AreEqual(expected, actual);
        }
    }
}
