using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Tests;

namespace ZMachine.Memory.Tests
{
    [TestClass]
    public class RoutineTests
    {
        [TestMethod]
        public void BasicRoutineTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            Routine routine = new Routine(new byte[] { 0x03,0x01,0x03,0x45,0x67,0x89,0x23,0x49,0x68,00}, 2);

            Assert.AreEqual(3, routine.Locals.Count);
            Assert.AreEqual(0x4567, routine.Locals[0]);
            Assert.AreEqual(0x8923, routine.Locals[1]);
            Assert.AreEqual(0x4968, routine.Locals[2]);
            Assert.AreEqual(9, routine.FirstInstructionAddress);
        }
    }
}
