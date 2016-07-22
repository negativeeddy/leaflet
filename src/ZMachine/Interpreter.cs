﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZMachine.Instructions;
using ZMachine.Memory;
using ZMachine.Story;

namespace ZMachine
{
    public class Interpreter
    {
        private const int UNUSED_RETURN_VALUE = -1;

        public void LoadStory(Stream storyStream)
        {
            MainMemory = new ZMemory(storyStream);
            FrameStack = new Stack<Routine>();
            LoadNewFrame(MainMemory.Header.PCStart - 1, 0, null);
        }

        public int ProgramCounter { get; set; }
        public ZMemory MainMemory { get; set; }

        public Stack<Routine> FrameStack { get; set; }

        public string ToInfoDumpFormat(ZOpcode zop)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{zop.BaseAddress:x4}:  ");
            for (int i = 0; i < zop.LengthInBytes; i++)
            {
                sb.Append(MainMemory.Bytes[zop.BaseAddress + i].ToString("x2"));
                sb.Append(' ');
            }

            sb.Append(' ', 31 - sb.Length); // print names at column 31

            sb.Append(zop.Definition.Name.ToUpper());

            sb.Append(' ', 47 - sb.Length);

            if (zop.Definition.Name == "jin")
            {
                foreach (var opr in zop.Operands)
                {
                    // print object names instead of values
                    int idx = GetOperandValue(opr);
                    var obj = MainMemory.ObjectTree.GetObject(idx);
                    sb.AppendFormat($"\"{obj.ShortName}\",");
                }
                sb.Remove(sb.Length - 1, 1);
            }
            else
            {
                for (int i = 0; i < zop.Operands.Count; i++)
                {
                    var opr = zop.Operands[i];
                    if (zop.Definition.UsesIndirection && i == 0)
                    {
                        sb.Append($"{new ZVariable((byte)GetOperandValue(opr)).ToInfoDumpFormat()},");
                    }
                    else
                    {
                        switch (opr.Type)
                        {

                            case OperandTypes.Variable:
                                switch (opr.Variable.Location)
                                {
                                    case ZVariableLocation.Global:
                                        sb.Append(opr.Variable.ToInfoDumpFormat());
                                        break;
                                    case ZVariableLocation.Local:
                                        sb.Append(opr.Variable.ToInfoDumpFormat());
                                        break;
                                    case ZVariableLocation.Stack:
                                        sb.Append(sb.Append(opr.Variable.ToInfoDumpFormat()));
                                        break;
                                }
                                break;
                            case OperandTypes.LargeConstant:
                                sb.Append($"#{opr.Constant:x4}");
                                break;
                            case OperandTypes.SmallConstant:
                                sb.Append($"#{opr.Constant:x2}");
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        sb.Append(' ');
                    }
                }
            }

            if (zop.Definition.HasStore)
            {
                sb.Append("-> ");
                switch (zop.Store.Location)
                {
                    case ZVariableLocation.Global:
                        sb.Append(zop.Store.ToInfoDumpFormat());
                        break;
                    case ZVariableLocation.Local:
                        sb.Append(zop.Store.ToInfoDumpFormat());
                        break;
                    case ZVariableLocation.Stack:
                        sb.Append(zop.Store.ToInfoDumpFormat(true));
                        break;
                }
            }

            if (zop.Definition.HasBranch)
            {
                string branchText = zop.BranchOffset.WhenTrue ? "TRUE" : "FALSE";
                switch (zop.BranchToAddress)
                {
                    case 0:
                        sb.Append($" [{branchText}] RFALSE");
                        break;
                    case 1:
                        sb.Append($" [{branchText}] RTRUE");
                        break;
                    default:
                        sb.Append($" [{branchText}] {zop.BranchToAddress:x4}");
                        break;
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Reads the value of a ZVariable
        /// </summary>
        /// <param name="variable">the variable to read</param>
        /// <param name="inPlace">if the value is in the evaluation stack, whether the read should pop values or read them in place. Otherwise
        /// it has no effect</param>
        /// <returns></returns>
        public ushort ReadVariable(ZVariable variable, bool inPlace = false)
        {
            ushort value;
            switch (variable.Location)
            {
                case ZVariableLocation.Global:
                    value = MainMemory.GlobalVariables[variable.Value];
                    DebugOutput($"  <= ReadVariable(0x{value:x2} <= G{variable.Value})", DebugLevel.Diagnostic);
                    break;
                case ZVariableLocation.Local:
                    value = CurrentRoutineFrame.Locals[variable.Value];
                    DebugOutput($"  <= ReadVariable(0x{value:x2} <= L{variable.Value})", DebugLevel.Diagnostic);
                    break;
                case ZVariableLocation.Stack:
                    var varStack = CurrentRoutineFrame.EvaluationStack;
                    if (inPlace)
                    {
                        value = varStack.Peek();
                        DebugOutput($"  <= ReadVariable(0x{value:x2} <= StackPeek)", DebugLevel.Diagnostic);
                    }
                    else
                    {
                        value = varStack.Pop();
                        DebugOutput($"  <= ReadVariable(0x{value:x2} <= StackPop)", DebugLevel.Diagnostic);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variable.Location));
            }
            return value;
        }

        /// <summary>
        /// Sets the value of a variable in memory
        /// </summary>
        /// <param name="variable">the variable to set</param>
        /// <param name="value">the value</param>
        /// <param name="inPlace">If setting a value on the stack, this indicates whether to change 
        /// the top value in place instead of pushing a new value. Has no effect on local or globals.</param>
        public void SetVariable(ZVariable variable, ushort value, bool inPlace = false)
        {
            switch (variable.Location)
            {
                case ZVariableLocation.Global:
                    MainMemory.GlobalVariables[variable.Value] = value;
                    DebugOutput($"  => SetVariable(0x{value:x2} => G{variable.Value})", DebugLevel.Diagnostic);
                    break;
                case ZVariableLocation.Local:
                    CurrentRoutineFrame.Locals[variable.Value] = value;
                    DebugOutput($"  => SetVariable(0x{value:x2} => L{variable.Value})", DebugLevel.Diagnostic);
                    break;
                case ZVariableLocation.Stack:
                    var varStack = CurrentRoutineFrame.EvaluationStack;
                    if (inPlace)
                    {
                        DebugOutput($"  => SetVariable(popstack first)", DebugLevel.Diagnostic);

                        varStack.Pop();
                    }
                    varStack.Push(value);
                    DebugOutput($"  => SetVariable(0x{value:x2} => stackPush)", DebugLevel.Diagnostic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variable.Location));
            }
        }

        enum DebugLevel { Off, Informational, Verbose, Diagnostic };
        private DebugLevel currentDebugOutputLevel = DebugLevel.Informational;

        private void DebugOutput(string output, DebugLevel level = DebugLevel.Informational)
        {
            if (level <= currentDebugOutputLevel)
            {
                Debug.WriteLine(output);
            }
        }

        public ZVariable Dereference(ZVariable zVarRef)
        {
            byte zvarVal = (byte)ReadVariable(zVarRef);
            return new ZVariable(zvarVal);
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
                    throw new NotImplementedException($"Reading from {operand.Type} not implemented");
            }
        }

        public string Print(bool printFrames = false)
        {
            StringBuilder sb = new StringBuilder();
            if (printFrames)
            {
                sb.AppendLine("---------------------");
                if (FrameStack.Count > 0)
                {

                    foreach (var frame in FrameStack)
                    {
                        sb.AppendLine(frame.ToString());
                    }
                }
                else
                {
                    sb.AppendLine("<no frames>");
                }

            }
            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);
            sb.Append(opcode.ToString());
            return sb.ToString();
        }

        private int instructionCount = 0;

        public void ExecuteCurrentInstruction()
        {
            ZOpcode opcode = new ZOpcode(MainMemory.Bytes, ProgramCounter);
            DebugOutput($"0x{instructionCount++:x4} {opcode}");

            switch (opcode.Definition.Name)
            {
                case "call":
                    Handle_Call(opcode);
                    break;
                case "ret": // ret value
                    Debug.Assert(opcode.OperandType.Count == 1);
                    // return value is the first opcode
                    ushort retVal = (ushort)GetOperandValue(opcode.Operands[0]);
                    Handle_Return(opcode, retVal);
                    break;
                case "ret_popped": // ret_popped
                    Debug.Assert(opcode.OperandType.Count == 0);
                    // Pops top of stack and returns that. (This is equivalent 
                    // to ret sp, but is one byte cheaper.) 
                    retVal = CurrentRoutineFrame.EvaluationStack.Pop();
                    Handle_Return(opcode, retVal);
                    break;
                case "rtrue":   // rtrue
                    Debug.Assert(opcode.OperandType.Count == 0);
                    // Return true (i.e., 1) from the current routine.
                    Handle_Return(opcode, 1);
                    break;
                case "rfalse":  // rfalse
                    Debug.Assert(opcode.OperandType.Count == 0);
                    // Return false (i.e., 0) from the current routine.
                    Handle_Return(opcode, 0);
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
                        // je a b c d ? (label)
                        short a = (short)GetOperandValue(op.Operands[0]);
                        for (int i = 1; i < op.Operands.Count; i++)
                        {
                            // is true if the first operand matches any of the others
                            if (a == GetOperandValue(op.Operands[i]))
                            {
                                return 1;
                            }
                        }
                        return 0;   // didn't match so don't branch
                    });
                    break;
                case "jz":  // jz a ?(label)
                    ExecInstruction(opcode, op => GetOperandValue(op.Operands[0]) == 0 ? 1 : 0);
                    break;
                case "jl":  // jl a b ?(label)
                    ExecInstruction(opcode, op => (short)GetOperandValue(op.Operands[0]) < (short)GetOperandValue(op.Operands[0]) ? 1 : 0);
                    break;
                case "jg":  // jg a b ?(label)
                    ExecInstruction(opcode, op => (short)GetOperandValue(op.Operands[0]) > (short)GetOperandValue(op.Operands[0]) ? 1 : 0);
                    break;
                case "jump":    // jump ?(label)
                    int jumpAddress = GetOperandValue(opcode.Operands[0]);
                    ProgramCounter = jumpAddress;
                    break;
                case "loadb":   // loadb array byte-index -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        int arrayAddress = GetOperandValue(op.Operands[0]);
                        int byteIndex = GetOperandValue(op.Operands[1]);
                        return MainMemory.Bytes[arrayAddress + byteIndex];
                    });
                    break;
                case "loadw":   // loadw array word-index -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        int arrayAddress = GetOperandValue(op.Operands[0]);
                        int wordIndex = GetOperandValue(op.Operands[1]);
                        var wob = new WordOverByteArray(MainMemory.Bytes, arrayAddress);
                        return wob[wordIndex];

                    });
                    break;
                case "storeb":  // storeb array byte-index value
                    ExecInstruction(opcode, op =>
                    {
                        int arrayAddress = GetOperandValue(op.Operands[0]);
                        int byteIndex = GetOperandValue(op.Operands[1]);
                        byte value = (byte)GetOperandValue(op.Operands[2]);
                        MainMemory.Bytes[arrayAddress + byteIndex] = value;
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "storew":  // storew array word-index value
                    ExecInstruction(opcode, op =>
                    {
                        int arrayAddress = GetOperandValue(op.Operands[0]);
                        int wordIndex = GetOperandValue(op.Operands[1]);
                        ushort value = (ushort)GetOperandValue(op.Operands[2]);

                        var wob = new WordOverByteArray(MainMemory.Bytes, arrayAddress);
                        wob[wordIndex] = value;
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "load":    // load (variable) -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        var actualZVar = GetDereferencedFirstZVar(op);
                        int value = ReadVariable(actualZVar, false);
                        return value;
                    });
                    break;
                case "store":   // store (variable) value
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        var actualZVar = GetDereferencedFirstZVar(op);
                        int valueToStore = GetOperandValue(op.Operands[1]);
                        SetVariable(actualZVar, (ushort)valueToStore, false);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "pull":    // pull (variable)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        var actualZVar = GetDereferencedFirstZVar(op);
                        int value = ReadVariable(actualZVar, true);
                        return value;
                    });
                    break;
                case "inc":    // inc (variable)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        var actualZVar = GetDereferencedFirstZVar(op);
                        short value = (short)ReadVariable(actualZVar, true);
                        value++;
                        SetVariable(actualZVar, (ushort)value, true);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "dec":    // dec (variable)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        var actualZVar = GetDereferencedFirstZVar(op);
                        short value = (short)ReadVariable(actualZVar, true);
                        value--;
                        SetVariable(actualZVar, (ushort)value, true);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "inc_chk":    // inc_chk (variable) value ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);

                        // do the increment
                        var actualZVar = GetDereferencedFirstZVar(op);
                        ushort value = ReadVariable(actualZVar, true);
                        value++;
                        SetVariable(actualZVar, value, true);

                        // do the compare
                        short compareVal = (short)GetOperandValue(op.Operands[1]);
                        return value == compareVal ? 1 : 0;
                    });
                    break;
                case "dec_chk":    // inc_chk (variable) value ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);

                        // do the decrement
                        var actualZVar = GetDereferencedFirstZVar(op);
                        short value = (short)ReadVariable(actualZVar, true);
                        value--;
                        SetVariable(actualZVar, (ushort)value, true);

                        // do the compare
                        short compareVal = (short)GetOperandValue(op.Operands[1]);
                        return value == compareVal ? 1 : 0;
                    });
                    break;
                case "jin":    // jin obj1 obj2 ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);

                        int childId = GetOperandValue(op.Operands[0]);
                        int parentId = GetOperandValue(op.Operands[1]);

                        return MainMemory.ObjectTree.IsValidChild(parentId, childId) ? 1 : 0;
                    });
                    break;
                case "get_child":    // get_child object -> (result) ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);

                        int objectId = GetOperandValue(op.Operands[0]);
                        return MainMemory.ObjectTree.GetChild(objectId);
                    });
                    break;
                case "get_sibling":    // get_sibling object -> (result) ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);

                        int objectId = GetOperandValue(op.Operands[0]);
                        return MainMemory.ObjectTree.GetSiblingId(objectId);
                    });
                    break;
                case "get_parent":    // get_parent object -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);

                        int objectId = GetOperandValue(op.Operands[0]);
                        return MainMemory.ObjectTree.GetParent(objectId);
                    });
                    break;
                case "insert_obj":  // insert_obj object destination
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        int objectId = GetOperandValue(op.Operands[0]);
                        int newParentId = GetOperandValue(op.Operands[1]);
                        MainMemory.ObjectTree.ReparentObject(objectId, newParentId);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print":   // print <literal-string>
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 0);
                        Debug.Assert(op.Definition.HasText == true);
                        // Print the quoted (literal) Z-encoded string.
                        Console.Write(opcode.Text);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print_obj":   // print_obj object
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // takes an object number and prints its short name
                        int objID = GetOperandValue(opcode.Operands[0]);
                        var obj = MainMemory.ObjectTree.GetObject(objID);
                        Console.Write(obj.ShortName);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print_addr":  // print_addr byte-address-of-string"
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // Print (Z-encoded) string at given byte address
                        int address = GetOperandValue(opcode.Operands[0]);
                        var obj = new ZStringBuilder(MainMemory.Bytes, address);
                        Console.Write(obj.ToString());
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print_paddr":  // print_paddr packed-address-of-string
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // Print (Z-encoded) string at given byte address
                        int address = GetOperandValue(opcode.Operands[0]) * 2;
                        var obj = new ZStringBuilder(MainMemory.Bytes, address);
                        Console.Write(obj.ToString());
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print_ret":  // print_ret <literal-string>
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 0);
                        Debug.Assert(op.Definition.HasText == true);
                        // Print the quoted (literal) Z-encoded string, then print
                        // a new-line and then return true (i.e., 1). 
                        Console.WriteLine(opcode.Text);
                        return 1;
                    });
                    break;
                case "print_num":  // print_num value
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // Print(signed) number in decimal.
                        int val = GetOperandValue(opcode.Operands[0]);
                        Console.Write(val.ToString("N"));
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "print_char":  // print_char output-character-code
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // Print a ZSCII character
                        int val = GetOperandValue(opcode.Operands[0]);
                        Debug.Assert(val >= 0 && val < 1023, "print_char out of range");
                        Console.Write(ZStringBuilder.GetChar((ushort)val));
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "new_line":  // new_line
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 0);
                        Console.WriteLine();
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "test":    // "test bitmap flags ? (label)"
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Jump if all of the flags in bitmap are set(i.e. if bitmap & flags == flags).
                        int bitmap = GetOperandValue(opcode.Operands[0]);
                        int flags = GetOperandValue(opcode.Operands[1]);
                        int result = (bitmap & flags) == flags ? 1 : 0;
                        return result;
                    });
                    break;
                case "test_attr": // test_attr object attribute ?(label)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Jump if object has attribute.
                        int objectID = GetOperandValue(opcode.Operands[0]);
                        BitNumber attribute = (BitNumber)GetOperandValue(opcode.Operands[1]);
                        ZObject obj = MainMemory.ObjectTree.GetObject(objectID);
                        int result = obj.HasAttribute(attribute) ? 1 : 0;
                        return result;
                    });
                    break;
                case "set_attr": // set_attr object attribute
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Make object have the attribute numbered attribute.
                        int objectID = GetOperandValue(opcode.Operands[0]);
                        BitNumber attribute = (BitNumber)GetOperandValue(opcode.Operands[1]);

                        ZObject obj = MainMemory.ObjectTree.GetObject(objectID);
                        obj.SetAttribute(attribute, true);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "clear_attr": // clear_attr object attribute
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Make object not have the attribute numbered attribute.
                        int objectID = GetOperandValue(opcode.Operands[0]);
                        BitNumber attribute = (BitNumber)GetOperandValue(opcode.Operands[1]);

                        ZObject obj = MainMemory.ObjectTree.GetObject(objectID);
                        obj.SetAttribute(attribute, false);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "and": // and a b -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Bitwise AND
                        int a = GetOperandValue(opcode.Operands[0]);
                        int b = GetOperandValue(opcode.Operands[1]);
                        return a & b;
                    });
                    break;
                case "or": // or a b -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Bitwise OR
                        int a = GetOperandValue(opcode.Operands[0]);
                        int b = GetOperandValue(opcode.Operands[1]);
                        return a | b;
                    });
                    break;
                case "pop": // pop
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 0);
                        // Throws away the top item on the stack. (This was useful 
                        // to lose unwanted routine call results in early Versions.)
                        CurrentRoutineFrame.EvaluationStack.Pop();
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "catch":
                    throw new NotImplementedException($"TODO: catch instruction not yet implemented");
                case "get_prop": // get_prop object property -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Read property from object (resulting in the default value if it had no such
                        // declared property). 
                        int objID = GetOperandValue(opcode.Operands[0]);
                        int propertyID = GetOperandValue(opcode.Operands[1]);

                        var obj = MainMemory.ObjectTree.GetObject(objID);
                        Debug.Assert(obj != null);

                        int value = (int)obj.GetPropertyValue(propertyID);
                        DebugOutput($"  Getting property {propertyID} of {obj.ShortName} (found 0x{value:x})");
                        return value;
                    });
                    break;
                case "put_prop":    // put_prop object property value
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 3);
                        // Writes the given value to the given property of the given object. 
                        // If the property does not exist for that object, the interpreter should halt with a suitable error message.
                        int objID = GetOperandValue(opcode.Operands[0]);
                        int propertyID = GetOperandValue(opcode.Operands[1]);
                        int value = GetOperandValue(opcode.Operands[2]);

                        var obj = MainMemory.ObjectTree.GetObject(objID);
                        Debug.Assert(obj != null);

                        obj.SetPropertyValue(propertyID, value);
                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "get_prop_len":    // get_prop_len property-address -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        // Writes the given value to the given property of the given object. 
                        // If the property does not exist for that object, the interpreter should halt with a suitable error message.
                        int propAddr = GetOperandValue(opcode.Operands[0]);

                        ZObjectProperty prop = new ZObjectProperty(MainMemory.Bytes, propAddr);
                        return prop.LengthInBytes;
                    });
                    break;
                case "get_prop_addr":   // get_prop_addr object property -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 2);
                        // Get the byte address (in dynamic memory) of the property data for the 
                        // given object's property. 
                        int objID = GetOperandValue(opcode.Operands[0]);
                        int propertyID = GetOperandValue(opcode.Operands[1]);

                        var obj = MainMemory.ObjectTree.GetObject(objID);

                        var prop = obj.CustomProperties.FirstOrDefault(p => p.ID == propertyID);
                        return prop?.BaseAddress ?? 0;
                    });
                    break;
                case "get_next_prop": // get_next_prop object property -> (result)
                    ExecInstruction(opcode, op =>
                    {
                        Debug.Assert(op.OperandType.Count == 1);
                        int objID = GetOperandValue(opcode.Operands[0]);
                        int propertyID = GetOperandValue(opcode.Operands[1]);

                        // Gives the number of the next property provided by the quoted object. 
                        // This may be zero, indicating the end of the property list; if called 
                        // with zero, it gives the first property number present. It is illegal 
                        // to try to find the next property of a property which does not exist, 
                        // and an interpreter should halt with an error message (if it can efficiently 
                        // check this condition). 
                        var obj = MainMemory.ObjectTree.GetObject(objID);

                        if (propertyID == 0) return obj.CustomProperties[0].ID;

                        return obj.CustomProperties.SkipWhile(p => p.ID != propertyID)
                                                    .Skip(1)
                                                    .FirstOrDefault()?.ID ?? 0;
                    });
                    break;
                case "set_colour":  // set_colour foreground background
                    ExecInstruction(opcode, op =>
                    {
                        throw new NotImplementedException("set_colour not implemented yet");
                        Debug.Assert(op.OperandType.Count == 2);
                        int foreground = GetOperandValue(opcode.Operands[0]);
                        int background = GetOperandValue(opcode.Operands[1]);

                        DebugOutput($"  Change color to fg 0x{foreground:x}, bg 0x{background:x}");

                        return UNUSED_RETURN_VALUE;
                    });
                    break;
                case "sread": // sread text parse
                    ExecInstruction(opcode, op => HandleRead(op));
                    break;
                default:
                    throw new NotImplementedException($"Opcode [{opcode}] not implemented yet");
            }
        }

        private ZVariable GetDereferencedFirstZVar(ZOpcode opcode)
        {
            Debug.Assert(opcode.OperandType.Count >= 1);

            // first instruction is a dereferenced ZVar
            byte zVarValue = (byte)GetOperandValue(opcode.Operands[0]);
            ZVariable zvarRef = new ZVariable(zVarValue);
            return zvarRef;
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
            int callAddress = GetOperandValue(opcode.Operands[0]);

            if (opcode.Operands[0].Type == OperandTypes.Variable)
            {
                // unpack the address
                callAddress = (int)AddressHelper.UnpackAddress((ushort)callAddress);
            }

            int nextInstruction = ProgramCounter += opcode.LengthInBytes;
            Debug.WriteLine($"call 0x{callAddress:x}");
            if (callAddress == 0)
            {
                SetVariable(opcode.Store, 0);
                ProgramCounter = nextInstruction;
            }
            else
            {
                LoadNewFrame(callAddress, nextInstruction, opcode.Store, opcode.Operands.Skip(1).ToArray());
            }
        }

        void Handle_Return(ZOpcode opcode, ushort returnValue)
        {
            // remove the current frame from the stack
            var oldFrame = FrameStack.Pop();

            // put the return value wherever the oldFrame required
            SetVariable(oldFrame.Store, (ushort)returnValue);

            DebugOutput($"  return  {returnValue:x}->{oldFrame.Store}");
            // update the instruction counter
            ProgramCounter = oldFrame.ReturnAddress;
        }

        private int HandleRead(ZOpcode op)
        {
            Debug.Assert(op.OperandType.Count == 2);
            int textBufferIdx = GetOperandValue(op.Operands[0]);
            int parseBufferIdx = GetOperandValue(op.Operands[1]);

            // copy the input to the text buffer
            int txtBufferSize = MainMemory.Bytes[textBufferIdx];
            if (txtBufferSize < 3)
            {
                throw new InvalidOperationException($"Text input buffer to small ({txtBufferSize} bytes)");
            }
            IList<byte> textBuffer = new ArraySegment<byte>(MainMemory.Bytes, textBufferIdx + 1, txtBufferSize + 1);

            string input = Console.ReadLine();
            if (input.Length > txtBufferSize)
            {
                // trim the input down to the buffer size
                input = input.Substring(0, txtBufferSize).ToLower();
            }

            for (int i = 0; i < input.Length; i++)
            {
                textBuffer[i] = (byte)input[i];   // copy to text buffer
            }
            textBuffer[input.Length] = 0;   // terminate text buffer with 0


            // parse the input

            int maxWordsParsed = MainMemory.Bytes[parseBufferIdx];
            int parseBufferSize = 4 * maxWordsParsed + 2;   // each parsed word takes 4 bytes
                                                            // 0 - dictionary index of the word
                                                            // 1 - number of letters in the word
                                                            // 4 - offset of the word in text buffer from textBufferIdx
            if (parseBufferSize < 6)
            {
                throw new InvalidOperationException($"Text input buffer to small ({parseBufferSize} bytes)");
            }
            IList<byte> parseBuffer = new ArraySegment<byte>(MainMemory.Bytes, parseBufferIdx, parseBufferSize);

            string[] words = SplitInput(input).ToArray();
            int wordsToParse = Math.Min(maxWordsParsed, words.Length);
            parseBuffer[1] = (byte)wordsToParse;

            int lastfoundWordIndex = 0;
            for (int i = 0; i < wordsToParse; i++)
            {
                string word = words[i];
                int dIndex = MainMemory.Dictionary.AddressOf(word);
                if (dIndex != -1)
                {
                    parseBuffer.SetWord((ushort)dIndex, 4 * i + 2);
                }
                else
                {
                    parseBuffer.SetWord(0, 4 * i + 2);
                }

                parseBuffer[4 * i + 4] = (byte)word.Length;

                int foundIndex = input.IndexOf(word, lastfoundWordIndex);
                lastfoundWordIndex = word.Length;
                parseBuffer[4 * i + 5] = (byte)(foundIndex + 1);
            }

            return UNUSED_RETURN_VALUE;
        }
        /// <summary>
        /// Split an input string into words as described in spec 13.6.1
        /// First, the text is broken up into words. Spaces divide up words and are otherwise
        /// ignored. Word separators also divide words, but each one of them is considered a 
        /// word in its own right. Thus, the erratically-spaced text "fred,go fishing" is 
        /// divided into four words: 
        /// fred / , / go / fishing
        /// </summary>
        /// <param name="input">the string to split</param>
        /// <returns>an array of words</returns>
        public IEnumerable<string> SplitInput(string input)
        {
            var spaceSplit = input.Split(' ');
            foreach (var word in spaceSplit)
            {

                if (word.Contains(','))
                {
                    int startIndex = 0;
                    int idx = word.IndexOf(',');
                    do
                    {
                        yield return word.Substring(startIndex, idx);
                        yield return ",";
                        startIndex = idx + 1;
                        idx = word.IndexOf(',', startIndex);
                    }
                    while (idx != -1);
                }
                else
                {
                    yield return word;
                }
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

        /// <summary>
        /// Executes the supplied handler method and then optionally stores the return
        /// value and then moves the instruction counter forward (either incrmentally or as a 
        /// valid branch).
        /// </summary>
        /// <param name="opcode">The opcode of the instruction which determines the Store and Branch
        /// functionality</param>
        /// <param name="handler">The primary logic of the instruction (minus Store and Branch
        /// functionality). If the opcode has a Store location, the return value from the handler
        /// will be put there. If the opcode is a branch, the return value is also used to determine
        /// whether the branch will happen</param>
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
                bool branchIfIfOne = branch.WhenTrue;

                // test the branch condition against the stored value
                if ((branchIfIfOne && branchValue != 0) ||
                    (!branchIfIfOne && branchValue == 0))
                {
                    switch (opcode.BranchToAddress)
                    {
                        // An offset of 0 means "return false from the current routine", and 1 means "return true from the current routine" (spec 4.7.1)
                        case 0:
                        case 1:
                            Handle_Return(opcode, (ushort)opcode.BranchToAddress);
                            break;
                        default:
                            DebugOutput($"  jump to 0x{opcode.BranchToAddress:x}");
                            ProgramCounter = opcode.BranchToAddress;
                            break;
                    }
                    return;
                }
            }

            // just move to the next instruction 
            ProgramCounter += opcode.LengthInBytes;
        }
    }
}
