using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ZMachine.Tests;

namespace ZMachine.Story.Tests
{
    [TestClass]
    public class ObjectTableTests
    {
        [TestMethod]
        public void ObjectTableVerificationTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var objTree = zm.MainMemory.ObjectTree;

            // mini zorkhas 178 objects
            int expectedEntrysCount = 179;
            int actualEntrysCount = zm.MainMemory.ObjectTree.Objects.Count();

            Assert.AreEqual(expectedEntrysCount, actualEntrysCount);

            Console.WriteLine("Found the following objects");
            foreach (var o in zm.MainMemory.ObjectTree.Objects)
            {
                Console.WriteLine($"{o.ID} at 0x{o.BaseAddress:X4} named {o.ShortName} with Properties at 0x{o.PropertyAddress:X4}");
            }

            var o2 = zm.MainMemory.ObjectTree.Objects.First();
            Assert.AreEqual(o2.ShortName, "forest");
            Assert.AreEqual(o2.ID, 1);
            Assert.AreEqual(o2.ParentID, 36);
            Assert.AreEqual(o2.PropertyAddress, 0x0a4f);

            o2 = zm.MainMemory.ObjectTree.Objects.Last();
            Assert.AreEqual(o2.ShortName, "pseudo");
            Assert.AreEqual(o2.ID, 179);
            Assert.AreEqual(o2.ParentID, 36);
            Assert.AreEqual(o2.PropertyAddress, 0x196c);

            o2 = zm.MainMemory.ObjectTree.Objects[16];
            Assert.AreEqual(o2.ShortName, "pair of candles");
            Assert.AreEqual(o2.ID, 17);
            Assert.AreEqual(o2.ParentID, 140);
            Assert.AreEqual(o2.PropertyAddress, 0x0bac);

            o2 = zm.MainMemory.ObjectTree.Objects[26];
            Assert.AreEqual(o2.ShortName, ZObject.UNNAMED_OBJECT_NAME);
            Assert.AreEqual(o2.ID, 27);
            Assert.AreEqual(o2.ParentID, 0);
            Assert.AreEqual(o2.PropertyAddress, 0x0c95);
        }

    }
}
