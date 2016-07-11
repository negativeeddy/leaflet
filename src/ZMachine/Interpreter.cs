using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Instructions;
using ZMachine.Memory;

namespace ZMachine
{
    public class Interpreter
    {
        public void LoadStory(Stream storyStream)
        {
            MainMemory = new ZMemory(storyStream);
            FrameStack = new Stack<Routine>();
            //ProgramCounter = (int)MainMemory.Header.PCStart;

            LoadNewFrame(MainMemory.Header.PCStart - 1, 0, null);
        }

        public int ProgramCounter { get; set; }
        public ZMemory MainMemory { get; set; }

        public ZProcessor Processor { get; set; }
        public Stack<Routine> FrameStack { get; set; }

        public int ReadVariable(ZVariable variable)
        {
            switch (variable.Location)
            {
                case ZVariableLocation.Global:
                    return MainMemory.GlobalVariables[variable.Value];
                case ZVariableLocation.Local:
                    return FrameStack.Peek().Locals[variable.Value];
                case ZVariableLocation.Stack:
                    return FrameStack.Peek().EvaluationStack.Peek();
                default:
                    throw new ArgumentOutOfRangeException(nameof(variable.Location));
            }
        }

        public void SetVariable(ZVariable variable, ushort value)
        {
            switch (variable.Location)
            {
                case ZVariableLocation.Global:
                    MainMemory.GlobalVariables[variable.Value] = value;
                    break;
                case ZVariableLocation.Local:
                    FrameStack.Peek().Locals[variable.Value] = value;
                    break;
                case ZVariableLocation.Stack:
                    FrameStack.Peek().EvaluationStack.Push(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variable.Location));
            }
        }

        public void Print(bool printFrames = false)
        {
            if (printFrames)
            {
                Console.WriteLine("---------------------");
                if (FrameStack.Count > 0)
                {

                    foreach (var frame in FrameStack)
                    {
                        Console.WriteLine(frame);
                    }
                }
                else
                {
                    Console.WriteLine("<no frames>");
                }

            }
            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);
            Console.WriteLine(opcode);
        }

        public void ExecuteCurrentInstruction()
        {
            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);
            switch (opcode.Definition.Name)
            {
                case "call":
                    Handle_Call(opcode);
                    break;
                case "ret":
                    Handle_Return(opcode);
                    break;
                case "add":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a + b;
                    });
                    break;
                case "sub":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a - b;
                    });
                    break;
                case "mul":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a * b;
                    });
                    break;
                case "div":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a / b;
                    });
                    break;
                case "mod":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a % b;
                    });
                    break;
                case "je":
                    ExecInstruction(opcode, op =>
                    {
                        short a = (short)GetOperandValue(op.Operands[0]);
                        short b = (short)GetOperandValue(op.Operands[1]);
                        return a == b ? 1 : 0;
                    });
                    break;
                default:
                    throw new NotImplementedException($"Opcode {opcode.Identifier}:{opcode.Definition.Name} not implemented yet");
            }
        }

        private Routine CurrentRoutineFrame
        {
            get
            {
                return FrameStack.Peek();
            }
        }

        void Handle_Call(ZOpcode opcode)
        {
            uint callAddress = opcode.Operands[0].Constant;
            if (callAddress == 0)
            {
                SetVariable(opcode.Store, 0);
                ProgramCounter++;
            }
            else
            {
                LoadNewFrame((int)callAddress, ProgramCounter + opcode.LengthInBytes, opcode.Store, opcode.Operands.Skip(1).ToArray());
            }
        }

        void Handle_Return(ZOpcode opcode)
        {
            // get the return value
            int retVal = GetOperandValue(opcode.Operands[0]);

            // remove the current frame from the stack
            var oldFrame = FrameStack.Pop();

            // put the return value wherever the oldFrame required
            SetVariable(oldFrame.Store, (ushort)retVal);

            // update the instruction counter
            ProgramCounter = oldFrame.ReturnAddress;
        }

        /// <summary>
        /// je a b ?(label)
        /// </summary>
        /// <param name="opcode"></param>
        private void Handle_je(ZOpcode opcode)
        {
            bool jump = true;
            int firstValue = GetOperandValue(opcode.Operands[0]);
            for (int i = 1; i < opcode.Operands.Count; i++)
            {
                if (firstValue == GetOperandValue(opcode.Operands[i]))
                {
                    jump = true;
                    break;
                }
            }

            if (jump)
            {
                ProgramCounter += opcode.BranchOffset.Offset;
            }
        }



        private void LoadNewFrame(int newAddress, int returnAddress, ZVariable returnStore, params ZOperand[] operands)
        {
            // initialize a new frame
            var initLocals = operands.Select(op => (ushort)GetOperandValue(op)).ToArray();
            Routine newRoutine = new Routine(MainMemory.Bytes, newAddress, returnAddress, returnStore, initLocals);
            FrameStack.Push(newRoutine);

            // update the instruction counter
            ProgramCounter = newRoutine.FirstInstructionAddress;
        }

        public int GetOperandValue(ZOperand operand)
        {
            switch (operand.Type)
            {
                case OperandTypes.LargeConstant:
                case OperandTypes.SmallConstant:
                    return (int)operand.Constant;
                case OperandTypes.Variable:
                    return ReadVariable(operand.Variable);
                default:
                    throw new NotImplementedException();
            }
        }

        private void ExecInstruction(ZOpcode opcode, Func<ZOpcode, int> handler)
        {
            int result = handler(opcode);

            if (opcode.Definition.HasStore)
            {
                SetVariable(opcode.Store, (ushort)result);
            }
            BranchOrNext(opcode, result);
        }

        /// <summary>
        /// Shifts the state to the next instruction depending on whether the 
        /// current opcode is a branching instruction or just needs to increment
        /// the instruction counter to the next instruction sequentially
        /// </summary>
        /// <param name="opcode"></param>
        private void BranchOrNext(ZOpcode opcode, int branchValue)
        {
            BranchOffset branch = opcode.BranchOffset;
            if (branch != null)
            {
                // read the resulting value from the opcode store variable
                int branchValue = ReadVariable(opcode.Store);
                bool branchIfNotZero = branch.WhenTrue;

                // test the branch condition against the stored value
                if ((branchIfNotZero && branchValue != 0) ||
                    (!branchIfNotZero && branchValue == 0))
                {
                    ProgramCounter = branch.Offset;
                    return;
                }
            }

            // just move to the next instruction 
            ProgramCounter += opcode.LengthInBytes;
        }
    }
}
