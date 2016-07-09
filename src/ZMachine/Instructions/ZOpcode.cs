using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Instructions
{

    enum OP1Bytecodes
    {
        OP1_128, OP1_129, OP1_130, OP1_131, OP1_132, OP1_133, OP1_134, OP1_135,
        OP1_136, OP1_137, OP1_138, OP1_139, OP1_140, OP1_141, OP1_142, OP1_143
    }

    enum OP0bytecodes
    {
        OP0_176, OP0_177, OP0_178, OP0_179, OP0_180, OP0_181, OP0_182, OP0_183,
        OP0_184, OP0_185, OP0_186, OP0_187, OP0_188, OP0_189, OP0_190, OP0_191
    }

    enum OP2Bytecodes
    {
        unused0, OP2_1, OP2_2, OP2_3, OP2_4, OP2_5, OP2_6, OP2_7,
        OP2_8, OP2_9, OP2_10, OP2_11, OP2_12, OP2_13, OP2_14, OP2_15,
        OP2_16, OP2_17, OP2_18, OP2_19, OP2_20, OP2_21, OP2_22, OP2_23,
        OP2_24, OP2_25, OP2_26, OP2_27, OP2_28, unused1, unused2, unused3
    }

    enum VARBytecodes
    {
        VAR_224, VAR_225, VAR_226, VAR_227, VAR_228, VAR_229, VAR_230, VAR_231,
        VAR_232, VAR_233, VAR_234, VAR_235, VAR_236, VAR_237, VAR_238, VAR_239,
        VAR_240, VAR_241, VAR_242, VAR_243, VAR_244, VAR_245, VAR_246, VAR_247,
        VAR_248, VAR_249, VAR_250, VAR_251, VAR_252, VAR_253, VAR_254, VAR_255
    }

    enum EXTBytecodes
    {
        EXT_0, EXT_1, EXT_2, EXT_3, EXT_4, EXT_5, EXT_6, EXT_7,
        EXT_8, EXT_9, EXT_10, EXT_11, EXT_12, EXT_13, EXT_14, unused0,
        EXT_16, EXT_17, EXT_18, EXT_19, EXT_20, EXT_21, EXT_22, EXT_23,
        EXT_24, EXT_25, EXT_26, EXT_27, EXT_28, EXT_29, unused1, unused2
    };


    public class ZOpcode
    {
        public OpcodeIdentifier Identifier { get { return new OpcodeIdentifier(OperandCount, Opcode); } }

        public OpcodeDefinition Definition { get { return OpcodeDefinition.GetKnownOpcode(Identifier); } }

        byte[] _bytes;
        int _baseAddress;

        public ZOpcode(byte[] data, int baseAddress)
        {
            _bytes = data;
            _baseAddress = baseAddress;
        }

        public ushort Opcode
        {
            get
            {
                switch (Form)
                {
                    case OpcodeForm.Short:
                        return _bytes[_baseAddress].FetchBits(BitNumber.Bit_3, 4);
                    case OpcodeForm.Long:
                    case OpcodeForm.Variable:
                        return _bytes[_baseAddress].FetchBits(BitNumber.Bit_4, 5);
                    case OpcodeForm.Extended:
                        return _bytes[_baseAddress + 1];
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
                byte opcodeType = _bytes[_baseAddress].FetchBits(BitNumber.Bit_7, 2);
                switch (opcodeType)
                {
                    case 0x02:
                        return _bytes[_baseAddress] == 0xBE ? OpcodeForm.Extended : OpcodeForm.Short;
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
                        byte bits = _bytes[_baseAddress].FetchBits(BitNumber.Bit_5, 2);
                        return (bits == 0x03) ? OperandCountType.OP0 : OperandCountType.OP1;
                    case OpcodeForm.Long:
                        return OperandCountType.OP2;
                    case OpcodeForm.Variable:
                        byte bits2 = _bytes[_baseAddress].FetchBits(BitNumber.Bit_5, 1);
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
                return Form == OpcodeForm.Extended || Form == OpcodeForm.Variable ? _baseAddress + 1 : _baseAddress;
            }
        }
        public IList<OperandTypes> OperandType
        {
            get
            {
                switch (OperandCount)
                {
                    case OperandCountType.OP0:
                        return new OperandTypes[] { OperandTypes.Omitted };
                    case OperandCountType.OP1:
                        var opType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_6, 2);
                        return new OperandTypes[] { opType };
                    case OperandCountType.OP2:
                        if (Form == OpcodeForm.Short)
                        {
                            return new OperandTypes[]
                            {
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_6, 1) == 1 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 1) == 1 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                            };
                        }else
                        {
                            return new OperandTypes[]
                            {
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_6, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                             _bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                            };
                        }
                    case OperandCountType.VAR:
                    case OperandCountType.EXT:
                        List<OperandTypes> list = new List<OperandTypes>(4);
                        OperandTypes oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_7, 2);
                        if (oprType == OperandTypes.Omitted)
                        {
                            return list.ToArray();
                        }
                        list.Add(oprType);
                        oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_5, 2);
                        if (oprType == OperandTypes.Omitted)
                        {
                            return list.ToArray();
                        }
                        list.Add(oprType);
                        oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_3, 2);
                        if (oprType == OperandTypes.Omitted)
                        {
                            return list.ToArray();
                        }
                        list.Add(oprType);
                        oprType = (OperandTypes)_bytes[OperandTypeAddress].FetchBits(BitNumber.Bit_1, 2);
                        if (oprType == OperandTypes.Omitted)
                        {
                            return list.ToArray();
                        }
                        list.Add(oprType);
                        return list.ToArray();
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public int LengthInBytes
        {
            get
            {
                if (Definition.HasText) return (TextAddr + TextSection.LengthInBytes) - _baseAddress;
                if (Definition.HasBranch) return (BranchOffsetAddr + BranchOffset.LengthInBytes) - _baseAddress;
                if (Definition.HasStore) return (StoreOffsetAddr + 1) - _baseAddress;
                // if none of the optional items exist, then the the StoreOffsetAddr is actually the next opcode in memory
                return StoreOffsetAddr - _baseAddress;

            }
        }

        private int OperandAddress
        {
            get
            {
                // in double VAR operand types are 2 bytes, others are 1 (spec 4.4.3.1)
                int sizeofOperandType = (Opcode == (ushort)12 || Opcode == (ushort)26) ? 2 : 1;
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
                                operand.Variable = new ZVariable()
                                {
                                    ID = _bytes.GetWord(operandAddr),
                                };
                                break;
                            case OperandTypes.LargeConstant:
                                operand.Constant = _bytes.GetWord(operandAddr);
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
                            ((Definition.Name == "call_vs2" || Definition.Name == "call_vn2") && _operands.Count <= 8)); // spec 4.5.1
                return _operands;
            }
        }

        private int StoreOffsetAddr
        {
            get
            {
                if (_operands == null)
                {
                    return OperandAddress;
                }
                else
                {
                    return OperandAddress + _operands.Sum(opr => opr.LengthInBytes);
                }
            }
        }
        public ZVariable Store
        {
            get
            {
                Debug.Assert(Definition.HasStore, "instruction does not have a store variable");
                if (Definition.HasStore)
                {
                    return new ZVariable { ID = _bytes[StoreOffsetAddr] };
                }
                else
                {
                    return null;
                }
            }
        }

        private int BranchOffsetAddr
        {
            get { return StoreOffsetAddr + (Definition.HasStore ? 1 : 0); }
        }

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

        private int TextAddr
        {
            get
            {
                return BranchOffsetAddr + BranchOffset.LengthInBytes;
            }
        }

        private ZStringBuilder _textSection;
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
            sb.Append($"{_baseAddress:x}: {Definition.Name}");
            foreach (var opr in Operands)
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

            if (this.Definition.HasStore)
            {
                sb.Append($" ->{Store}");
            }

            if (this.Definition.HasBranch)
            {
                sb.Append($" br {BranchOffset}");
            }

            return sb.ToString();
        }
    }
}
