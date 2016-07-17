﻿using System;
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
                Console.WriteLine($"{o.ID} at 0x{o.BaseAddress:X4} named {o.ShortName} with Properties at 0x{o.PropertyTableAddress:X4}");
            }

            var o2 = zm.MainMemory.ObjectTree.Objects.First();
            Assert.AreEqual(o2.ShortName, "forest");
            Assert.AreEqual(o2.ID, 1);
            Assert.AreEqual(o2.ParentID, 36);
            Assert.AreEqual(o2.PropertyTableAddress, 0x0a4f);

            o2 = zm.MainMemory.ObjectTree.Objects.Last();
            Assert.AreEqual(o2.ShortName, "pseudo");
            Assert.AreEqual(o2.ID, 179);
            Assert.AreEqual(o2.ParentID, 36);
            Assert.AreEqual(o2.PropertyTableAddress, 0x196c);

            o2 = zm.MainMemory.ObjectTree.Objects[16];
            Assert.AreEqual(o2.ShortName, "pair of candles");
            Assert.AreEqual(o2.ID, 17);
            Assert.AreEqual(o2.ParentID, 140);
            Assert.AreEqual(o2.PropertyTableAddress, 0x0bac);

            o2 = zm.MainMemory.ObjectTree.Objects[29];
            Assert.AreEqual("you", o2.ShortName);
            Assert.AreEqual(30, o2.ID);
            Assert.AreEqual(0, o2.ParentID);
            Assert.AreEqual(0, o2.SiblingID);
            Assert.AreEqual(0, o2.ChildID);

            o2 = zm.MainMemory.ObjectTree.Objects[26];
            Assert.AreEqual(o2.ShortName, ZObject.UNNAMED_OBJECT_NAME);
            Assert.AreEqual(o2.ID, 27);
            Assert.AreEqual(o2.ParentID, 0);
            Assert.AreEqual(o2.PropertyTableAddress, 0x0c95);
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

            foreach (var item in data)
            {
                bool isValid = objTree.IsValidChild(item.Item1, item.Item2);
                Assert.AreEqual(item.Item3, isValid, $"{item.Item1} is not valid parent of {item.Item2}");
            }

        }

        [TestMethod]
        public void TestObjectReader()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var objTree = zm.MainMemory.ObjectTree;
            var obj = objTree[170];
            Console.WriteLine($"Loaded Object =>");
            Console.WriteLine(obj.ToFullString());
            //170.Attributes: 7
            //    Parent object:  27  Sibling object:  56  Child object: 175
            //    Property address: 18c2
            //    Description: "Thief's Lair"
            //     Properties:
            //         [22] 38
            //         [18] 57 40
            //         [14] 5e 24
            //         [12] 76
            //         [9] 00 14

            Assert.AreEqual(170, obj.ID);
            Assert.AreEqual(27, obj.ParentID);
            Assert.AreEqual(56, obj.SiblingID);
            Assert.AreEqual(175, obj.ChildID);
            Assert.AreEqual(0x18c2, obj.PropertyTableAddress);

            Assert.AreEqual(5, obj.CustomProperties.Length);

            ValidateProperty(obj.CustomProperties[0], 22, new byte[] { 0x38, });
            ValidateProperty(obj.CustomProperties[1], 18, new byte[] { 0x57, 0x40 });
            ValidateProperty(obj.CustomProperties[2], 14, new byte[] { 0x5e, 0x24 });
            ValidateProperty(obj.CustomProperties[3], 12, new byte[] { 0x76 });
            ValidateProperty(obj.CustomProperties[4], 9, new byte[] { 0x00, 0x14 });
        }

        private void ValidateProperty(ZObjectProperty property, int ID, byte[] Data)
        {
            Assert.AreEqual(ID, property.ID);
            Assert.AreEqual(Data.Length, property.Data.Count);

            var compares = Data.Zip(property.Data, (expected, actual) => new { expected, actual });
            foreach (var item in compares)
            {
                Assert.AreEqual(item.expected, item.actual);
            }
        }

        [TestMethod]
        public void InsertObjectTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var objTree = zm.MainMemory.ObjectTree;

            var youId = 30;
            var westOfHouseId = 46;

            var youObj = objTree.GetObject(youId);
            var westOfHouseObj = objTree.GetObject(westOfHouseId);

            // verify initial values
            Assert.AreEqual(0, youObj.ParentID);
            Assert.AreEqual(0, youObj.SiblingID);
            Assert.AreEqual(0, youObj.ChildID);

            Assert.AreEqual(27, westOfHouseObj.ParentID);
            Assert.AreEqual(0, westOfHouseObj.SiblingID);
            Assert.AreEqual(82, westOfHouseObj.ChildID);

            objTree.ReparentObject(youId, westOfHouseId);

            youObj = objTree.GetObject(youId);
            westOfHouseObj = objTree.GetObject(westOfHouseId);

            Assert.AreEqual(westOfHouseId, youObj.ParentID);
            Assert.AreEqual(82, youObj.SiblingID);
            Assert.AreEqual(0, youObj.ChildID);

            Assert.AreEqual(27, westOfHouseObj.ParentID);
            Assert.AreEqual(0, westOfHouseObj.SiblingID);
            Assert.AreEqual(youId, westOfHouseObj.ChildID);

        }

    }
}
