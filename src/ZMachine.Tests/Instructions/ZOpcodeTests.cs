using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ZMachine.Tests;

namespace ZMachine.Instructions.Tests
{
    [TestClass()]
    public class ZOpcodeTests
    {
        [TestMethod]
        public void VarOperandTypeTest()
        {
            byte b = Convert.ToByte("00101111", 2);

            ZOperand operand = new ZOperand(OperandTypes.Variable);
        }

        [TestMethod()]
        public void OpCodeTest_insert_ob()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3803; // "3803: insert_obj g73 g00
            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            //byte[] bytes = new byte[] { 0x6e, 0x83, 0x10, 0xe0, 0x3f, 0x2c };
            // 0110 1110    Form Long, 2OP
            //              opcode 01110 => 14
            //              bit 6 = 1 => operand = VAR
            //              bit 5 = 1 => operand = VAR
            // 1000 0011    Operand1, 0x83
            // 0001 0000    Operand2, 0x10

            // 1110 0000
            // 0011 1111
            // 0010 1010

            ZVariable[] operandValues = new ZVariable[] {
                new ZVariable( 0x83),
                new ZVariable(0x10),
            };

            //ZOpcode zop = new ZOpcode(bytes, 0);
            Console.WriteLine(zop);
            Assert.AreEqual(OpcodeForm.Long, zop.Form, "Form");
            Assert.AreEqual(14, zop.Opcode, "Opcde");
            Assert.AreEqual(2, zop.OperandType.Count, "OperandTypes Count");  // Long form is always 2 operands
            Assert.AreEqual(3, zop.LengthInBytes);


            for (int i = 0; i < zop.OperandType.Count; i++)
            {
                Assert.AreEqual(OperandTypes.Variable, zop.OperandType[i], $"OperandTypes {i}");
                Assert.AreEqual(operandValues[i].Location, zop.Operands[i].Variable.Location, $"Operand values {i}");
                Assert.AreEqual(operandValues[i].Value, zop.Operands[i].Variable.Value, $"Operand values {i}");

            }

            string stringConversion = "3803: insert_obj g73 g0";
            Assert.AreEqual(stringConversion, zop.ToString());
        }

        [TestMethod()]
        public void OpCodeTest_je()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3b5d; // "3b5d: je local3 local2 ?~3b77
            string expectedStringConversion = "3b5d: je local3 local2 ?~3b77";
            // 0x61,0x04,0x03,0x58,0x55,0x45
            // 0110 0001
            // 0000 0100   var local 3
            // 0000 0011   var local 2
            // 0101 1000   branch
            // 0101 0101

            int expectedOpcode = 0x01;
            OpcodeForm expectedForm = OpcodeForm.Long;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x04) },
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x03) },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 4;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);

            Assert.AreEqual(0x3b77.ToString("x"), zop.BranchToAddress.ToString("x"));
        }

        [TestMethod()]
        public void OpCodeTest_jz()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3b65; // 3b65: jz local1 ?3b6c
            string expectedStringConversion = "3b65: jz local1 ?3b6c";
            // 0xa0 0x02 0xc6
            // 1010 0000    short form, OP1, opcode 0
            // 0000 0010    var local 1
            // 1100 0110    branch 

            int expectedOpcode = 0;
            OpcodeForm expectedForm = OpcodeForm.Short;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x02) },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 3;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);

            Assert.AreEqual(0x3b6c.ToString("x"), (zop.BranchToAddress).ToString("x"));
        }

        [TestMethod()]
        public void OpCodeTest_call()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3b3d; // "3b3d: call 3b4a local0 ->local2
            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            int expectedOpcode = 0x00;
            OpcodeForm expectedForm = OpcodeForm.Variable;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.LargeConstant) { Constant = 0x3b4a },
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x01) },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 6;
            string expectedStringConversion = "3b3d: call 3b4a local0 ->local2";

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);
        }

        [TestMethod()]
        public void OpCodeTest_jump()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3816; // "3816: jump ffc2
            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            //byte[] bytes = new byte[] { 0x8c, 0xff, 0xc2};
            // 1000 1010    Form SHORT, 1OP
            //              opcode 1010 => 0x0c
            //              bits 5,4 = 00 -> Large Constant Operand
            // 1111 1111    Operand1 = 0xffc2
            // 1010 0010    (contributes to large constant)

            int expectedOpcode = 0x0c;
            OpcodeForm expectedForm = OpcodeForm.Short;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.LargeConstant) { Constant = 0x37d9 },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 3;
            string expectedStringConversion = "3816: jump 37d9";

            CompareOpcodeWithExpectedValues(
                zop, 
                expectedOpcode, 
                expectedForm, 
                expectedOperands, 
                expectedLengthInBytes, 
                expectedStringConversion);
        }

        private static void CompareOpcodeWithExpectedValues(ZOpcode zop, int expectedOpcode, OpcodeForm expectedForm, ZOperand[] expectedOperands, int expectedLengthInBytes, string expectedStringConversion)
        {
            Console.WriteLine($"Checking the following opcode: {zop}");

            // check basic form
            Assert.AreEqual(expectedForm, zop.Form, "Form is wrong");
            Assert.AreEqual(expectedOpcode, zop.Opcode, "Opcode is wrong");

            // check operands
            int expectedOperandCount = expectedOperands.Length;
            Assert.AreEqual(expectedOperandCount, zop.OperandType.Count, "OperandTypes Count is wrong");  // Long form is always 2 operands

            for (int i = 0; i < zop.Operands.Count; i++)
            {
                Assert.AreEqual(expectedOperands[i].Type, zop.OperandType[i], $"OperandTypes[{i}] is wrong");
                switch(expectedOperands[i].Type)
                {
                    case OperandTypes.Variable:
                        Assert.AreEqual(expectedOperands[i].Variable.Location, zop.Operands[i].Variable.Location, $"Operand{i} VAR location is wrong");
                        Assert.AreEqual(expectedOperands[i].Variable.Value, zop.Operands[i].Variable.Value, $"Operand{i} VAR value is wrong");
                        break;
                    case OperandTypes.LargeConstant:
                    case OperandTypes.SmallConstant:
                        Assert.AreEqual(expectedOperands[i].Constant, zop.Operands[i].Constant, $"Operand{i} {expectedOperands[i].Type} value is wrong");
                        break;
                    case OperandTypes.Omitted:
                        Assert.Fail("Found an omitted operand. Omitted operands should not be created.");
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid Operand{i} Type detected {expectedOperands[i].Type}");
                }
            }

            // check overall length
            Assert.AreEqual(expectedLengthInBytes, zop.LengthInBytes, "Length Test");

            // check text output
            Assert.AreEqual(expectedStringConversion, zop.ToString());
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
            while (instructionPointer <= lastOpcode)
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
                new Tuple<int, string>(0x37d9, "37d9: call 3b36 3e88 ffff ->sp"),
                new Tuple<int, string>(0x37e2, "37e2: storew sp 00 01"),
                new Tuple<int, string>(0x37e7, "37e7: call 3b36 4e50 28 ->sp"),
                new Tuple<int, string>(0x37ef, "37ef: call 3b36 4792 96 ->sp"),
                new Tuple<int, string>(0x37f7, "37f7: store 10 2e"),
                new Tuple<int, string>(0x37fa, "37fa: store 8a a7"),
                new Tuple<int, string>(0x37fd, "37fd: store 36 01"),
                new Tuple<int, string>(0x3800, "3800: store 83 1e"),
                new Tuple<int, string>(0x3803, "3803: insert_obj g73 g0"),
                new Tuple<int, string>(0x3806, "3806: call 5862 ->sp"),
                new Tuple<int, string>(0x380b, "380b: new_line"),
                new Tuple<int, string>(0x380c, "380c: call 61f4 ->sp"),
                new Tuple<int, string>(0x3811, "3811: call 381a ->sp"),
                new Tuple<int, string>(0x3816, "3816: jump 37d9"),
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