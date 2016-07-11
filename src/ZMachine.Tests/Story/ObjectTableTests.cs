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

        [TestMethod]
        public void TestObjectIsValidChild()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var objTree = zm.MainMemory.ObjectTree;

            var data = new Tuple<int, int, bool>[]  // Tuple<parentId, childId, result>
            {
                new Tuple<int, int,bool>(0,27,true),   // <unnamed> has no parent
                new Tuple<int, int,bool>(27,121,true), // Thiefs lair's parent is <unnamed>
                new Tuple<int, int,bool>(64,166,true),   // trunk of jewel's parent is Resevoir
                new Tuple<int, int,bool>(143,122,true),  // tan label's parent is magic boat
                new Tuple<int, int,bool>(122,143,false),     // object is child, not parent (backwards relationship)
                new Tuple<int, int,bool>(45,21,false),    // grating's grandparent is <unnamed>, not parent
                new Tuple<int, int,bool>(21,46,false),    // chute is not related to West of House
                new Tuple<int, int,bool>(97,68,false),    // glass bottle is sibling of brown sack
                new Tuple<int, int,bool>(113,113,false),    // both are nasty knife
            };

            foreach(var item in data)
            {
                bool isValid = objTree.IsValidChild(item.Item1, item.Item2);
                Assert.AreEqual(item.Item3, isValid, $"{item.Item1} is not valid parent of {item.Item2}");
            }

        }
    }
}
