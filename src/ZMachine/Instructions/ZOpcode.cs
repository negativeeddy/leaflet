using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Instructions
{
    public class ZOpcode
    {
        public OpcodeIdentifier Identifier { get { return new OpcodeIdentifier(OperandCount, Opcode); } }

        public OpcodeDefinition Definition { get { return OpcodeDefinition.GetKnownOpcode(Identifier); } }

        byte[] _bytes;
        public int BaseAddress { get; }

        /// <summary>
        /// The raw bytes that make up the instruction.
        /// </summary>
        public IList<byte> Bytes
        {
            get
            {
                return new ArraySegment<byte>(_bytes, BaseAddress, LengthInBytes);
            }
        }

        public ZOpcode(byte[] data, int baseAddress)
        {
            _bytes = data;
            this.BaseAddress = baseAddress;
        }

        public ushort Opcode
        {
            get
            {
                switch (Form)
                {
                    case OpcodeForm.Short:
                        return _bytes[BaseAddress].FetchBits(BitNumber.Bit_3, 4);
                    case OpcodeForm.Long:
                    case OpcodeForm.Variable:
                        return _bytes[BaseAddress].FetchBits(BitNumber.Bit_4, 5);
                    case OpcodeForm.Extended:
                        return _bytes[BaseAddress + 1];
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public OpcodeForm Form
        {
            get
            {
                // From the spec section 4.3
                byte opcodeType = _bytes[BaseAddress].FetchBits(BitNumber.Bit_7, 2);
                switch (opcodeType)
                {
                    case 0x02:
                        return _bytes[BaseAddress] == 0xBE ? OpcodeForm.Extended : OpcodeForm.Short;
                    case 0x03:
                        return OpcodeForm.Variable;
                    default:
                        return OpcodeForm.Long;
                }
            }
        }

        public OperandCountType OperandCount
        {
            get
            {
                switch (Form)
                {
                    case OpcodeForm.Short:
                        byte bits = _bytes[BaseAddress].FetchBits(BitNumber.Bit_5, 2);
                        return (bits == 0x03) ? OperandCountType.OP0 : OperandCountType.OP1;
                    case OpcodeForm.Long:
                        return OperandCountType.OP2;
                    case OpcodeForm.Variable:
                        byte bits2 = _bytes[BaseAddress].FetchBits(BitNumber.Bit_5, 1);
                        return (bits2 == 0x00) ? OperandCountType.OP2 : OperandCountType.VAR;
                    case OpcodeForm.Extended:
                        return OperandCountType.VAR;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private int OperandTypeAddress
        {
            get
            {
                // Operands succeed the opcode. Extended opcodes are 2 bytes, others are 1 (spec 4.3.4)
                return Form == OpcodeForm.Extended || Form == OpcodeForm.Variable ? BaseAddress + 1 : BaseAddress;
            }
        }

        public IList<OperandTypes> OperandType
        {
            get
            {
                OperandTypes[] types;

                switch (OperandCount)
                {
                    case OperandCountType.OP0:
                        types = new OperandTypes[] { };
                        break;
                    case OperandCountType.OP1:
                        Debug.Assert(Form == OpcodeForm.Short, "Is OP1 only in short form?");
                        var opType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 2);
                        types = new OperandTypes[] { opType };
                        break;
                    case OperandCountType.OP2:
                        if (Form == OpcodeForm.Short)
                        {
                            types = new OperandTypes[]
                            {
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_6, 1) == 1 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 1) == 1 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                            };
                        }
                        else
                        {
                            types = new OperandTypes[]
                            {
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_6, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                            };
                        }
                        break;
                    case OperandCountType.VAR:
                    case OperandCountType.EXT:
                        List<OperandTypes> list = new List<OperandTypes>(4);

                        OperandTypes oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_7, 2);
                        if (oprType != OperandTypes.Omitted)
                        {
                            list.Add(oprType);

                            oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 2);
                            if (oprType != OperandTypes.Omitted)
                            {
                                list.Add(oprType);

                                oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_3, 2);
                                if (oprType != OperandTypes.Omitted)
                                {
                                    list.Add(oprType);

                                    oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_1, 2);
                                    if (oprType != OperandTypes.Omitted)
                                    {
                                        list.Add(oprType);
                                    }
                                }
                            }
                        }
                        types = list.ToArray();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return types;
            }
        }

        public int LengthInBytes
        {
            get
            {
                // find the last item in the opcode and return ((item address + item length) - base address)
                if (Definition.HasText) return (TextAddr + TextSection.LengthInBytes) - BaseAddress;
                if (Definition.HasBranch) return (BranchOffsetAddr + BranchOffset.LengthInBytes) - BaseAddress;
                if (Definition.HasStore) return (StoreOffsetAddr + 1) - BaseAddress;
                // if none of the optional items exist, then the the StoreOffsetAddr is actually the next opcode in memory
                return StoreOffsetAddr - BaseAddress;
            }
        }

        private int OperandAddress
        {
            get
            {
                // in double VAR opcodes (call_vs2 and call_vn2) operand types are 2 bytes, others are 1 (spec 4.4.3.1)
                int sizeofOperandType;
                if ((Form == OpcodeForm.Variable) && (Opcode == (ushort)12 || Opcode == (ushort)26))
                {
                    sizeofOperandType = 2;
                }
                else
                {
                    sizeofOperandType = 1;
                }

                return OperandTypeAddress + sizeofOperandType;
            }
        }

        private IList<ZOperand> _operands = null;
        public IList<ZOperand> Operands
        {
            get
            {
                if (_operands == null)
                {
                    _operands = new List<ZOperand>();

                    int operandAddr = OperandAddress;

                    for (int i = 0; i < OperandType.Count; i++)
                    {
                        var operandType = OperandType[i];

                        var operand = new ZOperand(operandType);
                        // load the data for the operand
                        switch (operandType)
                        {
                            case OperandTypes.Variable:
                                operand.Variable = new ZVariable(_bytes[operandAddr]);
                                break;
                            case OperandTypes.LargeConstant:
                                if (Definition.IsCall && i == 0)
                                {
                                    operand.Constant = _bytes.GetWordUnpacked(operandAddr);  // unpack addresses for call routines
                                }
                                else if (Definition.Name == "jump")
                                {
                                    // the value is stored as an unsigned 16 bit offset, but is more useful to use to store
                                    // as a 16-bit absolute value;
                                    operand.Constant = (ushort)(_bytes.GetWord(operandAddr) + BaseAddress + 1);
                                }
                                else
                                {
                                    operand.Constant = _bytes.GetWord(operandAddr);
                                }
                                break;
                            case OperandTypes.SmallConstant:
                                operand.Constant = _bytes[operandAddr];
                                break;
                            case OperandTypes.Omitted:
                                // ignore
                                break;

                        }

                        _operands.Add(operand);

                        operandAddr += operand.LengthInBytes;
                    }
                }

                Debug.Assert(_operands.Count <= 4 ||
                            ((Definition.Name == "call_vs2" || Definition.Name == "call_vn2") && _operands.Count <= 8), "Multi VAR operands not supported yet"); // spec 4.5.1


                if (_operands.Count(opr => opr.Type == OperandTypes.Variable && opr.Variable.Location == ZVariableLocation.Stack) > 1)
                {
                    throw new NotImplementedException("Multiple stack variables in an opcode are not yet supported");
                }
                return _operands;
            }
        }

        /// <summary>
        /// Address of the Store portion of the instruction (if any)
        /// </summary>
        private int StoreOffsetAddr
        {
            get
            {
                if (Operands == null)
                {
                    return OperandAddress;
                }
                else
                {
                    return OperandAddress + Operands.Sum(opr => opr.LengthInBytes);
                }
            }
        }

        /// <summary>
        /// The Store data for the instruction (if any).
        /// This data indicates where the result of the instruction will be put
        /// when it is complete
        /// </summary>
        public ZVariable Store
        {
            get
            {
                Debug.Assert(Definition.HasStore, "instruction does not have a store variable");
                if (Definition.HasStore)
                {
                    return new ZVariable(_bytes[StoreOffsetAddr]);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Address of the branch portion of the instruction (if any)
        /// </summary>
        private int BranchOffsetAddr
        {
            get { return StoreOffsetAddr + (Definition.HasStore ? 1 : 0); }
        }

        /// <summary>
        /// The absolute address to branch to (if any)
        /// </summary>
        public int BranchToAddress
        {
            get
            {
                if (!Definition.HasBranch)
                {
                    throw new InvalidOperationException("Opcode has not branch data");
                }
                // spec 4.7.2
                return (BaseAddress + LengthInBytes) + BranchOffset.Offset - 2;
            }
        }

        /// <summary>
        /// The Branch data (if any)
        /// </summary>
        public BranchOffset BranchOffset
        {
            get
            {
                if (Definition.HasBranch)
                {
                    IList<byte> branchData = new ArraySegment<byte>(_bytes, BranchOffsetAddr, 2);
                    return new BranchOffset(branchData);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The address of the Text portion of the instruction (if any)
        /// </summary>
        private int TextAddr
        {
            get
            {
                return BranchOffsetAddr + BranchOffset.LengthInBytes;
            }
        }

        private ZStringBuilder _textSection;

        /// <summary>
        /// The data for the Text portion of the instruction (if any)
        /// </summary>
        private ZStringBuilder TextSection
        {
            get
            {
                if (_textSection == null && Definition.HasText)
                {
                    _textSection = new ZStringBuilder(_bytes, TextAddr);

                }
                return _textSection;
            }
        }

        /// <summary>
        /// String representation of the text section of the instruction (if any)
        /// </summary>
        public string Text
        {
            get
            {
                return TextSection?.ToString();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            // print the address & operand name
            sb.Append($"{BaseAddress:x4}: {Definition.Name}");

            // print the operands
            for(int i=0; i<Operands.Count; i++)
            {
                var opr = Operands[i];

                if (Definition.UsesIndirection && i == 0)
                {
                    switch (opr.Type)
                    {

                        case OperandTypes.Variable:
                            sb.Append($" {new ZVariable((byte)opr.Constant):x2}");
                            break;
                        case OperandTypes.LargeConstant:
                            sb.Append($" {new ZVariable((byte)opr.Constant):x2}");
                            break;
                        case OperandTypes.SmallConstant:
                            sb.Append($" {new ZVariable((byte)opr.Constant):x2}");
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    switch (opr.Type)
                    {

                        case OperandTypes.Variable:
                            sb.Append($" {opr.Variable:x}");
                            break;
                        case OperandTypes.LargeConstant:
                            sb.Append($" {opr.Constant:x4}");
                            break;
                        case OperandTypes.SmallConstant:
                            sb.Append($" {opr.Constant:x2}");
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            // print the optional store
            if (this.Definition.HasStore)
            {
                sb.Append($" ->{Store}");
            }

            // print the optional branch info
            if (this.Definition.HasBranch)
            {
                sb.Append($" ?");
                if (!BranchOffset.WhenTrue)
                {
                    sb.Append('~');
                }
                sb.Append($"{BranchToAddress:x}");
            }

            return sb.ToString();
        }
    }
}
