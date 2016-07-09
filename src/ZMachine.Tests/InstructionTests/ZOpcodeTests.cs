using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ZMachine.Instructions;
using ZMachine.Tests;

namespace ZMachine.Memory.Tests
{
    [TestClass()]
    public class ZOpcodeTests
    {
        [TestMethod]
        public void VarOperandTypeTest()
        {
            byte b = Convert.ToByte("00101111", 2);

        }

        [TestMethod()]
        public void OpCodeTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3803; // "3803: insert_obj g73 g00
            
            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            Assert.AreEqual(OpcodeForm.Short, zop.Form, "Form");
            Assert.AreEqual(14, zop.Opcode, "Opcde");
            Assert.AreEqual(2, zop.OperandType.Count, "OperandTypes Count");
            for (int i = 0; i < zop.OperandType.Count; i++)
            {
                Assert.AreEqual(OperandTypes.Variable, zop.OperandType[i], $"OperandTypes {i}");
            }
        }


        [TestMethod]
        public void ExplicitOpcodeDecoderTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            Tuple<int, string>[] testData = GetOpcodeInputData();

            // test opcode output with explicit addresses
            foreach (var item in testData)
            {
                ZOpcode oc = new ZOpcode(zm.MainMemory.Bytes, item.Item1);
                Console.WriteLine(oc);
                Assert.AreEqual(item.Item2, oc.ToString(), $"Bad decode at address 0x{item.Item1:x4}");
            }
        }

        [TestMethod]
        public void SequenctialOpcodeDecoderTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            Tuple<int, string>[] testData = GetOpcodeInputData();

            // test opcode output & lengths
            int firstOpcode = testData.First().Item1;
            int lastOpcode = testData.Last().Item1;

            int instructionPointer = firstOpcode;
            int instructionCount = 0;
            while (instructionPointer < lastOpcode)
            {
                var opcode = new ZOpcode(zm.MainMemory.Bytes, instructionPointer);
                Console.WriteLine(opcode);
                instructionCount++;
                instructionPointer += opcode.LengthInBytes;
            }
            Assert.AreEqual(testData.Length, instructionCount);
        }

        private static Tuple<int, string>[] GetOpcodeInputData()
        {
            // this set of instructions in minizork are sequential in memory
            Tuple<int, string>[] testData = new Tuple<int, string>[]
            {
                                   // address, instruction text
                new Tuple<int, string>(0x37d9, "37d9: call 1d9b 3e88 ffff ->sp"),
                new Tuple<int, string>(0x37e2, "37e2: storew sp 00 01"),
                new Tuple<int, string>(0x37e7, "37e7: call 1d9b 4e50 28 ->sp"),
                new Tuple<int, string>(0x37ef, "37ef: call 1d9b 4792 96 ->sp"),
                new Tuple<int, string>(0x37f7, "37f7: store 10 2e"),
                new Tuple<int, string>(0x37fa, "37fa: store 8a a7"),
                new Tuple<int, string>(0x37fd, "37fd: store 36 01"),
                new Tuple<int, string>(0x3800, "3800: store 83 1e"),
                new Tuple<int, string>(0x3803, "3803: insert_obj g73 g00"),
                new Tuple<int, string>(0x3806, "3806: call 2c31 ->sp"),
                new Tuple<int, string>(0x380b, "380b: new_line"),
                new Tuple<int, string>(0x380c, "380c: call 30fa ->sp"),
                new Tuple<int, string>(0x3811, "3811: call 1c0d ->sp"),
                new Tuple<int, string>(0x3816, "3816: jump ffc2"),
            };
            return testData;
        }

        [TestMethod]
        public void KnownOpcodeTest()
        {
            string opcodeIDString = "VAR_224";
            var id = new OpcodeIdentifier(opcodeIDString);
            var opcode = OpcodeDefinition.GetKnownOpcode(id);

            Assert.AreEqual("call", opcode.Name, opcodeIDString);
            Assert.AreEqual(0, opcode.ID.OpcodeNumber, opcodeIDString);
            Assert.AreEqual(true, opcode.HasStore, opcodeIDString);
            Assert.AreEqual(false, opcode.HasBranch, opcodeIDString);

            opcodeIDString = "OP0_181";
            id = new OpcodeIdentifier(opcodeIDString);
            opcode = OpcodeDefinition.GetKnownOpcode(id);

            Assert.AreEqual("save", opcode.Name, opcodeIDString);
            Assert.AreEqual(5, opcode.ID.OpcodeNumber, opcodeIDString);
            Assert.AreEqual(false, opcode.HasStore, opcodeIDString);
            Assert.AreEqual(true, opcode.HasBranch, opcodeIDString);

            opcodeIDString = "OP2_25";
            id = new OpcodeIdentifier(opcodeIDString);
            opcode = OpcodeDefinition.GetKnownOpcode(id);

            Assert.AreEqual("call_s2", opcode.Name, opcodeIDString);
            Assert.AreEqual(0x19, opcode.ID.OpcodeNumber, opcodeIDString);
            Assert.AreEqual(true, opcode.HasStore, opcodeIDString);
            Assert.AreEqual(false, opcode.HasBranch, opcodeIDString);

            opcodeIDString = "OP1_130";
            id = new OpcodeIdentifier(opcodeIDString);
            opcode = OpcodeDefinition.GetKnownOpcode(id);

            Assert.AreEqual("get_child", opcode.Name, opcodeIDString);
            Assert.AreEqual(2, opcode.ID.OpcodeNumber, opcodeIDString);
            Assert.AreEqual(true, opcode.HasStore, opcodeIDString);
            Assert.AreEqual(true, opcode.HasBranch, opcodeIDString);
        }
    }
}