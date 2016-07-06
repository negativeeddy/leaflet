using System;
using System.Collections.Generic;
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
        public OpcodeIdentifier Identifier { get { return new OpcodeIdentifier { OperandCount = this.OperandCount, OpcodeNumber = Opcode }; } }

        public OpcodeDefinition Definition {  get { return OpcodeDefinition.GetKnownOpcode(Identifier); } }

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
                        return _bytes[0].FetchBits(BitNumber.Bit_4, 4);
                    case OpcodeForm.Long:
                    case OpcodeForm.Variable:
                        return _bytes[0].FetchBits(BitNumber.Bit_5, 5);
                    case OpcodeForm.Extended:
                        return _bytes[1];
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
                byte opcodeType = _bytes[0].FetchBits(BitNumber.Bit_7, 2);
                switch (opcodeType)
                {
                    case 0x02:
                        return _bytes[0] == 0xBE ? OpcodeForm.Extended : OpcodeForm.Short;
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
                        byte bits = _bytes[0].FetchBits(BitNumber.Bit_5, 2);
                        return (bits == 0x03) ? OperandCountType.OP0 : OperandCountType.OP1;
                    case OpcodeForm.Long:
                        return OperandCountType.OP2;
                    case OpcodeForm.Variable:
                        byte bits2 = _bytes[0].FetchBits(BitNumber.Bit_5, 1);
                        return (bits2 == 0x00) ? OperandCountType.OP2 : OperandCountType.VAR;
                    case OpcodeForm.Extended:
                        return OperandCountType.VAR;
                    default:
                        throw new InvalidOperationException();
                }
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
                        var opType = (OperandTypes)_bytes[1].FetchBits(BitNumber.Bit_6, 2);
                        return new OperandTypes[] { opType };
                    case OperandCountType.OP2:
                        return new OperandTypes[]
                        {
                             _bytes[1].FetchBits(BitNumber.Bit_6, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                             _bytes[1].FetchBits(BitNumber.Bit_5, 1) == 0 ? OperandTypes.SmallConstant : OperandTypes.Variable,
                        };
                    case OperandCountType.VAR:
                        return new OperandTypes[]
                        {
                            (OperandTypes)_bytes[1].FetchBits(BitNumber.Bit_4, 2),
                            (OperandTypes)_bytes[1].FetchBits(BitNumber.Bit_2, 2),
                        };
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public int Length
        {
            get
            {
                int length = (Form == OpcodeForm.Extended ? 2 : 1);   // extended OpCodes use 2 bytes for the opcode, all others use 1

                // add byte count for operand type bytes, if any
                switch (Form)
                {
                    case OpcodeForm.Short:
                    case OpcodeForm.Long:
                        // no extra byte
                        break;
                    case OpcodeForm.Extended:
                    case OpcodeForm.Variable:
                        length++;
                        break;
                }

                // add length of operands
                length += Operands.Sum(o => o.LengthInBytes);

                if (Definition.HasStore)
                {
                    length++;
                }

                if (Definition.HasBranch)
                {
                    length += 2;
                }

                return length;
            }
        }

        private int OperandBaseAddress
        {
            get { return Form == OpcodeForm.Extended ? _baseAddress + 1 : _baseAddress; }
        }

        private IList<ZOperand> _operands = null;
        public IList<ZOperand> Operands
        {
            get
            {
                if (_operands == null)
                {
                    _operands = new List<ZOperand>();

                    int operandAddr = OperandBaseAddress;

                    for (int i = 0; i < OperandType.Count; i++)
                    {
                        var operand = new ZOperand()
                        {
                            Type = OperandType[i],
                            Variable = new ZVariable()
                            {
                                Data = _bytes.GetWord(operandAddr),
                            }
                        };

                        _operands.Add(operand);

                        operandAddr += operand.LengthInBytes;
                    }
                }
                return _operands;
            }
        }

        public ushort BranchOffset { get; set; }

        public byte Store { get; set; }

        string Text { get; set; }


    
    }

    public struct BranchInstruction
    {
        public BranchType Type;
        public ushort Address;
    }

    public enum BranchType
    {
        ReturnTrue,
        ReturnFalse,
        BranchAddress
    }
}
