using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using NegativeEddy.Leaflet.TestHelpers;

namespace NegativeEddy.Leaflet.Story.Tests
{
    [TestClass]
    public class DictionaryTests
    {
        [TestMethod]
        public void DictionaryEntryTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var dictionary = zm.MainMemory.Dictionary;

            // mini zork has 3 Separators
            int expectedEntryCount = 3;
            int actualEntryCount = dictionary.Separators.Count();

            Assert.AreEqual(expectedEntryCount, actualEntryCount);

            // mini zork's Separators are (in order) a full stop, a comma and a double-quote
            string expectedSeparators = new string(new char[] { '.', ',', '"' });
            string actualSeparators = new string(zm.MainMemory.Dictionary.Separators);

            Assert.AreEqual(expectedSeparators, actualSeparators);
        }

        [TestMethod]
        public void DictionaryWordTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var dictionary = zm.MainMemory.Dictionary;

            // mini zork has 536 Separators
            int expectedEntryCount = 536;
            int actualEntryCount = dictionary.Words.Count();

            Assert.AreEqual(expectedEntryCount, actualEntryCount);

            // mini zork's dictionary first 14 items are are (in order) 
            var expectedEntrys = new string[] { "$ve", ".", ",", "#comm", "#rand", "#reco", "#unre", "\"", "a", "about", "across", "activa", "advent", "again" };
            var actualEntrys = zm.MainMemory.Dictionary.Words.Take(14).ToArray();

            Assert.AreEqual(expectedEntrys.Length, actualEntrys.Length, "Wrong number of Dictionary entries found");

            for (int i = 0; i < expectedEntrys.Length; i++)
            {
                Console.WriteLine($"{expectedEntrys[i]}, {actualEntrys[i]}");
                Assert.AreEqual(expectedEntrys[i], actualEntrys[i]);
            }
        }
    }
}
