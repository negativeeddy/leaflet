using System;
using System.Collections.Generic;
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

        public void Print()
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
            Console.WriteLine();

            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);

            Console.WriteLine(opcode);
            Console.WriteLine("---------------------");
        }

        public void ExecuteCurrentInstruction()
        {
            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);
            switch (opcode.Definition.Name)
            {
                case "call":
                    Handle_Call(opcode);
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

        private void LoadNewFrame(int newAddress, int returnAddress, ZVariable returnStore, params ZOperand[] operands)
        {
            var initLocals = operands.Select(op => (ushort)GetOperandValue(op)).ToArray();
            Routine newRoutine = new Routine(MainMemory.Bytes, newAddress, returnAddress, returnStore, initLocals);
            FrameStack.Push(newRoutine);
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
    }
}
