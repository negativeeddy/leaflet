using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            Assert.Inconclusive();
            var inputs = new List<Tuple<byte[], OpcodeForm, ushort, OperandTypes[]>>()
            {
                new Tuple<byte[], OpcodeForm, ushort, OperandTypes[]>(new byte[] { 0x01, 0x02, 0x00 }, OpcodeForm.Short, 0x0102, new [] { OperandTypes.Omitted }),
            };

            foreach (var input in inputs)
            {
                byte[] data = input.Item1;
                ZOpcode zop = new ZOpcode(data, 0);

                OpcodeForm form = input.Item2;
                ushort opcode = input.Item3;
                OperandTypes[] operandTypes = input.Item4;

                Assert.AreEqual(form, zop.Form, "Form");
                Assert.AreEqual(opcode, zop.Opcode, "Opcde");
                Assert.AreEqual(operandTypes.Length, zop.OperandType.Count, "OperandTypes Count");
                for (int i = 0; i < zop.OperandType.Count; i++)
                {
                    Assert.AreEqual(operandTypes[i], zop.OperandType[i], $"OperandTypes {i}");
                }
            }
        }

        [TestMethod]
        public void OpcodeDecoderTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            // zork mini has at 0x37d9 should be "call 1d9b 3e88 ffff ->sp"
            int startAddress = 0x37d9;
            int endAddress = 0x3816;
            int instructionPointer = startAddress;
                
            ZOpcode oc = new ZOpcode(zm.MainMemory.Bytes, instructionPointer);
            Console.WriteLine(oc);
            //Assert.AreEqual("call", oc.Definition.Name );

            //Assert.AreEqual("37D9: call 1D9B 3E88 FFFF ->sp", oc.ToString());

            while(instructionPointer < endAddress)
            {
                instructionPointer += oc.LengthInBytes;
                oc = new ZOpcode(zm.MainMemory.Bytes, instructionPointer);
                Console.WriteLine(oc);
            }
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