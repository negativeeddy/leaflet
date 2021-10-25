using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NegativeEddy.Leaflet.Story;
using NegativeEddy.Leaflet.TestHelpers;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Instructions.Tests
{
    public struct OpcodeData
    {
        public int Address;
        public string Name;
        public ZOperand[] Operands;
        public int LengthInBytes;
        public int? BranchToAddress;
        public ZVariable Store;

        public bool BranchToOnTrue { get; internal set; }
    }

    [TestClass()]
    public class ZOpcodeTests
    {
        [TestMethod()]
        public void WalkthroughTestMiniZork_Mailbox()
        {
            ValidateGameWalkThrough(
                @"GameFiles\minizork.z3",
                @"GameFiles\miniZork_input_mailbox.txt",
                @"GameFiles\miniZork_opcodes_mailbox.dasm"
            );
        }

        [TestMethod()]
        public void WalkthoughTestMiniZork_Walkthrough()
        {
            ValidateGameWalkThrough(
                @"GameFiles\minizork.z3",
                @"GameFiles\miniZork_input_walkthrough.txt",
                @"GameFiles\miniZork_opcodes_walkthrough.dasm"
            );
        }

        public void ValidateGameWalkThrough(string gameFile, string inputFile, string expectedOutput)
        {
            var zm = ZMachineLoader.Load(gameFile);
            zm.Input = new InputFeeder(File.ReadLines(inputFile));
            zm.Output.Subscribe(x => Console.Write(x));
            zm.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Diagnostic;

            var randData = new ShimRandomNumberGenerator();
            zm.RandomNumberGenerator = (IRandomNumberGenerator)randData;

            ConcurrentQueue<string> diagQueue = new ConcurrentQueue<string>();

            try
            {
                zm.Diagnostics.Subscribe(x => Debug.Write(x));// diagQueue.Enqueue(x));
                string[] input = File.ReadAllLines(expectedOutput);

                var instructionBlocks = input.BufferUntil(x => x.StartsWith("0x"));


                int index = -1; // keep track of the last instruction index
                foreach (var block in instructionBlocks)
                {
                    // validate the instruction index and address from the first line
                    var addressParts = block.First().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                    index = Convert.ToInt32(addressParts[0], 16);
                    int address = Convert.ToInt32(addressParts[1], 16);

                    Assert.AreEqual(address, zm.ProgramCounter, $"Instruction 0x{index:x4} address is 0x{zm.ProgramCounter:x} instead of 0x{address:x}");

                    // check if a random number was generated before executing the instruction
                    string nextLine = block.Skip(1).FirstOrDefault();
                    if (nextLine!= null && nextLine.StartsWith("random_generated"))
                    {
                        int nextRand = Convert.ToInt32(nextLine.Split(' ')[1], 16);
                        randData.NextRandomNumberToGenerate = nextRand;
                    }

                    zm.ExecuteCurrentInstruction();

                    // validate the rest of the requirements after the instruction is executed
                    foreach (string validation in block.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        string[] parts = validation.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                        var currentFrame = zm.FrameStack.Peek();

                        switch (parts[0])
                        {
                            case "current_globals":  // validate the current local variables
                                Assert.AreEqual(parts.Length - 1, zm.MainMemory.GlobalVariables.Count,
                                    $"Global variable count is wrong after executing instruction x{index:x4}");

                                // zip the two together and return true if any have a mismatch when correlated
                                var compareGlobals = parts.Skip(1).Zip(zm.MainMemory.GlobalVariables, (expected, actual) =>
                                                                                expected != actual.ToString("x4"));
                                Assert.IsFalse(compareGlobals.Any(x => x), $"Global don't match after executing instruction 0x{index:x4}");
                                break;
                            case "current_locals":  // validate the current local variables
                                Assert.AreEqual(parts.Length - 1, currentFrame.Locals.Count,
                                    $"Local variable count is wrong after executing instruction x{index:x4}");

                                // zip the two together and return true if any have a mismatch when correlated
                                var compareLocals = parts.Skip(1).Zip(currentFrame.Locals, (expected, actual) =>
                                                                                expected != actual.ToString("x4"));
                                Assert.IsFalse(compareLocals.Any(x => x), $"Locals don't match after executing instruction 0x{index:x4}");
                                break;
                            case "current_stack":  // validate the current local variables
                                Assert.AreEqual(parts.Length - 1, currentFrame.EvaluationStack.Count,
                                    $"Local variable count is wrong at instruction x{index:x4}");

                                // zip the two together and return true if any have a mismatch when correlated
                                var compareStackVars = parts.Skip(1).Zip(currentFrame.EvaluationStack, (expected, actual) =>
                                                                                expected != actual.ToString("x4"));
                                Assert.IsFalse(compareStackVars.Any(x => x), $"Stack doesn't match after executing instruction 0x{index:x4}");
                                break;
                            case "object":
                                // object ID:63 Name:large_bag Attributes:11,6 Parent:112 Sibling:0 Child:0 PropertyAddr:0f88 Properties:[18],3f,78,[17],29,48,[16],f4,f3
                                int id = int.Parse(parts[1].Split(':')[1]);
                                ZObject obj = zm.MainMemory.ObjectTree.GetObject(id);
                                string expectedObj = obj.ToLongString();
                                string actualObj = validation.Substring(7);
                                Assert.AreEqual(expectedObj, actualObj, $"object {id} is invalid after instruction 0x{index:x4}");
                                break;
                            case "memory_dump":
                                int memoryAddress = Convert.ToInt32(parts[1], 16);
                                byte[] bytesExpected = parts.Skip(2).Select(x => Convert.ToByte(x, 16)).ToArray();
                                byte[] bytesActual = zm.MainMemory.Bytes.Skip(memoryAddress).Take(bytesExpected.Length).ToArray();

                                for (int i = 0; i < bytesActual.Length; i++)
                                {
                                    Assert.AreEqual(bytesExpected[i], bytesActual[i], $"Memory location 0x{memoryAddress + 1:x4} is wrong");
                                }
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
            finally
            {
                DumpDiagQueue(diagQueue);
            }
        }



        private static void DumpDiagQueue(ConcurrentQueue<string> diagQueue)
        {
            foreach (var diagLine in diagQueue.Skip(diagQueue.Count - 50))
            {
                Console.Write(diagLine);
            }
        }

        public OpcodeData ParseInfoDump(string opcodeLine, ZMemory memory)
        {
            OpcodeData opcodeData = new OpcodeData();

            string tmp = opcodeLine.Substring(1, 4);
            opcodeData.Address = Convert.ToInt32(tmp, 16);
            tmp = opcodeLine.Substring(32, 48 - 32).TrimEnd().ToLower();
            opcodeData.Name = tmp;

            var operandParts = opcodeLine.Substring(48).Split(' ', ',');
            List<ZOperand> operandList = new List<ZOperand>();
            for (int i = 0; i < operandParts.Length; i++)
            {
                ZOperand zopr = OperandFromString(operandParts[i]);

                if (zopr != null)
                {
                    operandList.Add(zopr);
                }
                else
                {
                    if (operandParts[i][0] == '"')
                    {
                        string name = operandParts[i].Substring(1, operandParts[i].Length - 1);
                        while (operandParts[i].Last() != '"')
                        {
                            i++;
                            name += " " + operandParts[i];
                        }
                        name = name.Substring(0, name.Length - 1);
                        var obj = memory.ObjectTree.Objects.First(o => o.ShortName == name);
                        int objId = obj.ID;
                        operandList.Add(new ZOperand(OperandTypes.SmallConstant) { Constant = (byte)objId });
                    }
                    if (operandParts[i][0] == '[')
                    {
                        if (operandParts[i].Substring(1, 4) == "TRUE") { opcodeData.BranchToOnTrue = true; }
                        else { opcodeData.BranchToOnTrue = false; }

                        i++;

                        if (operandParts[i] == "RTRUE") { opcodeData.BranchToAddress = 1; }
                        else if (operandParts[i] == "RFALSE") { opcodeData.BranchToAddress = 0; }
                        else opcodeData.BranchToAddress = Convert.ToInt32(operandParts[i], 16);

                    }
                    else if (operandParts[i] == "->")
                    {
                        i++;
                        var tmpopr = OperandFromString(operandParts[i]);
                        opcodeData.Store = tmpopr.Variable;

                    }
                }

            }

            opcodeData.Operands = operandList.ToArray();

            return opcodeData;
        }

        private ZOperand OperandFromString(string zOperand)
        {
            if (zOperand[0] == 'L')
            {
                int lVal = int.Parse(zOperand.Substring(1));
                return new ZOperand(OperandTypes.Variable) { Variable = new ZVariable((byte)(1 + lVal)) };
            }
            else if (zOperand[0] == 'G')
            {
                int gVal = int.Parse(zOperand.Substring(1));
                return new ZOperand(OperandTypes.Variable) { Variable = new ZVariable((byte)(0x3a + gVal)) };
            }
            else if (zOperand[0] == '#')
            {
                uint constant = uint.Parse(zOperand.Substring(1));
                if (constant > 0xff)
                {
                    return new ZOperand(OperandTypes.LargeConstant) { Constant = constant };
                }
                else
                {
                    return new ZOperand(OperandTypes.SmallConstant) { Constant = (byte)constant };
                }
            }
            else if (zOperand.Contains("SP"))
            {
                return new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x00) };
            }
            return null;
        }

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
        public void OpCodeTest_print()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x5865; // "5865: print  MINI-ZORK I:"
            string expectedStringConversion = "5865: print MINI-ZORK I: ";
            // 0xb2
            // 1011 0010 Form = short, OP2, opcode 10010 = 0x12

            int expectedOpcode = 0x02;
            OpcodeForm expectedForm = OpcodeForm.Short;
            ZOperand[] expectedOperands = new ZOperand[] { };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 17;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);
        }

        [TestMethod()]
        public void OpCodeTest_and()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x58d4; // "58d4: and sp 07ff ->sp"
            string expectedStringConversion = "58d4: and sp 07ff ->sp";
            // 0xc9, 0x8f, 0x00, 0x07, 0xff, 0x00         
            // 1100 1001 Form = variable, OP2, opcode 01001 = 0x09 
            // 1000 1111 VAR, LConst, Omit, Omit 
            // 0000 0000 1st operand VAR
            // 0000 0111 2nd operand LConst part 1
            // 1111 1111 2nd operand LConst part 2
            // 0000 0000 Store

            int expectedOpcode = 0x09;
            OpcodeForm expectedForm = OpcodeForm.Variable;
            ZOperand[] expectedOperands = new ZOperand[] {
                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x00) },
                new ZOperand(OperandTypes.LargeConstant) { Constant = 0x07ff},
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 6;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            var tmp = zop.OperandType;
            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);
        }




        [TestMethod()]
        public void OpCodeTest_je_6d3f()
        {
            // 6d3f:  c1 ab 83 01 00 68       JE              G73,L00,(SP)+ [FALSE] 6d6b
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x6d3f; // je global73 local0 (sp)+ ?~6d6b
            string expectedStringConversion = "6d3f: je g73 local0 sp ?~6d6b";
            // 0xc1 0xab 0x83 0x01 0x00 0x68 
            // 1100 0001   Form = Var, OP_2, opcode 0x01, (has branch)
            // 1010 1011   op types: VAR, VAR, VAR, omitted
            ///1010 0111    global 
            // 0000 0001   var local 1
            // 0000 0000   sp
            // 0110 1000   branch 

            int expectedOpcode = 0x01;
            OpcodeForm expectedForm = OpcodeForm.Variable;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x83) },
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x01) },
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x00) },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 6;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);

            Assert.AreEqual(0x6d6b.ToString("x"), zop.BranchToAddress.ToString("x"));
            Assert.AreEqual(false, zop.BranchOffset.WhenTrue, "Branch direction");
        }

        [TestMethod()]
        public void OpCodeTest_get_sibling_6dbd()
        {
            // 6dbd:  a1 04 04 bf ab          GET_SIBLING     L03 -> L03 [TRUE] 6d6b
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x6dbd; // get_sibling l03 (sp)+ ?~6d6b
            string expectedStringConversion = "6dbd: get_sibling local3 ->local3 ?6d6b";
            // 0xa1 0x04 0x04 0xbf 0xab
            // 1010 0001   Form = Short, OP_1, opcode 0x01 operand type, VAR
            // 0000 0100   VAR  3
            ///0000 0100   STORE 3 
            // 1011 1111   Branch
            // 1010 1011   sp

            int expectedOpcode = 1;
            OpcodeForm expectedForm = OpcodeForm.Short;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.Variable) { Variable = new ZVariable(0x04) },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 5;

            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);

            Assert.IsTrue(zop.Definition.HasStore);
            Assert.AreEqual(ZVariableLocation.Local, zop.Store.Location);
            Assert.AreEqual(3, zop.Store.Value);


            Assert.IsTrue(zop.Definition.HasBranch);
            Assert.AreEqual(0x6d6b.ToString("x"), zop.BranchToAddress.ToString("x"));
            Assert.AreEqual(true, zop.BranchOffset.WhenTrue, "Branch direction");
        }

        [TestMethod()]
        public void OpCodeTest_je_3b5d()
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

        public void OpCodeTest_store()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            int address = 0x3816; // 37f7: store 10 2e
            ZOpcode zop = new ZOpcode(zm.MainMemory.Bytes, address);
            string expectedStringConversion = "37f7: store 10 2e";

            //byte[] bytes = new byte[] { 0x8c, 0xff, 0xc2};
            // 1000 1010    Form SHORT, 1OP
            //              opcode 1010 => 0x0c
            //              bits 5,4 = 00 -> Large Constant Operand
            // 1111 1111    Operand1 = 0xffc2
            // 1010 0010    (contributes to large constant)

            int expectedOpcode = 0x0d;
            OpcodeForm expectedForm = OpcodeForm.Short;
            ZOperand[] expectedOperands = new ZOperand[] {
                                                new ZOperand(OperandTypes.LargeConstant) { Constant = 0x37d9 },
            };
            int expectedOperandCount = expectedOperands.Length;
            int expectedLengthInBytes = 3;

            CompareOpcodeWithExpectedValues(
                zop,
                expectedOpcode,
                expectedForm,
                expectedOperands,
                expectedLengthInBytes,
                expectedStringConversion);
        }

        private void CompareOpcodeWithExpectedValues(ZOpcode zop, string expectedOpcodeName, ZOperand[] expectedOperands,
                                                        int expectedLengthInBytes, int? branchToAddress, ZVariable store)
        {
            Console.WriteLine($"Checking the following opcode: {zop}");

            Assert.AreEqual(expectedOpcodeName, zop.Definition.Name, "Opcode name is wrong");

            // check operands
            int expectedOperandCount = expectedOperands.Length;
            Assert.AreEqual(expectedOperandCount, zop.OperandType.Count, "OperandTypes Count is wrong");  // Long form is always 2 operands

            for (int i = 0; i < zop.Operands.Count; i++)
            {
                Assert.AreEqual(expectedOperands[i].Type, zop.OperandType[i], $"OperandTypes[{i}] is wrong");
                switch (expectedOperands[i].Type)
                {
                    case OperandTypes.Variable:
                        Assert.AreEqual(expectedOperands[i].Variable.Location, zop.Operands[i].Variable.Location, $"Operand{i} VAR has wrong location");
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
            // Assert.AreEqual(expectedLengthInBytes, zop.LengthInBytes, "Length Test");

            // check branch condition
            if (branchToAddress == null)
            {
                Assert.IsFalse(zop.Definition.HasBranch);
            }
            else
            {
                Assert.IsTrue(zop.Definition.HasBranch, "HasBranch is wrong");
                Assert.AreEqual(branchToAddress.Value, zop.BranchToAddress, "Branch address wrong");
            }

            if (store == null)
            {
                Assert.IsFalse(zop.Definition.HasStore);
            }
            else
            {
                Assert.IsTrue(zop.Definition.HasStore);
                Assert.AreEqual(store.Location, zop.Store.Location);
                Assert.AreEqual(store.Value, zop.Store.Value);
            }
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
                switch (expectedOperands[i].Type)
                {
                    case OperandTypes.Variable:
                        Assert.AreEqual(expectedOperands[i].Variable.Location, zop.Operands[i].Variable.Location, $"Operand{i} VAR has wrong location");
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

            var testData = GetOpcodeInputData();

            // test opcode output with explicit addresses
            foreach (var item in testData)
            {
                ZOpcode oc = new ZOpcode(zm.MainMemory.Bytes, item.address);
                Console.WriteLine(oc);
                Assert.AreEqual(item.intructionText, oc.ToString(), $"Bad decode at address 0x{item.address:x4}");
            }
        }

        [TestMethod]
        public void SequenctialOpcodeDecoderTest()
        {
            string filename = @"GameFiles\minizork.z3";
            var zm = ZMachineLoader.Load(filename);

            var testData = GetOpcodeInputData();

            // test opcode output & lengths
            int firstOpcode = testData.First().address;
            int lastOpcode = testData.Last().address;

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

        private static (int address, string intructionText)[] GetOpcodeInputData()
        {
            // this set of instructions in minizork are sequential in memory
            (int, string)[] testData = new (int, string)[]
            {
                                   // address, instruction text
                (0x37d9, "37d9: call 3b36 3e88 ffff ->sp"),
                (0x37e2, "37e2: storew sp 00 01"),
                (0x37e7, "37e7: call 3b36 4e50 28 ->sp"),
                (0x37ef, "37ef: call 3b36 4792 96 ->sp"),
                (0x37f7, "37f7: store g0 2e"),
                (0x37fa, "37fa: store g7a a7"),
                (0x37fd, "37fd: store g26 01"),
                (0x3800, "3800: store g73 1e"),
                (0x3803, "3803: insert_obj g73 g0"),
                (0x3806, "3806: call 5862 ->sp"),
                (0x380b, "380b: new_line"),
                (0x380c, "380c: call 61f4 ->sp"),
                (0x3811, "3811: call 381a ->sp"),
                (0x3816, "3816: jump 37d9"),
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