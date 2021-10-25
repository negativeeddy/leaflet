using System.Diagnostics;

namespace NegativeEddy.Leaflet.Instructions;

/// <summary>
/// Operands reference either a Constant value or a Variable
/// </summary>
public class ZOperand
{
    public OperandTypes Type { get; }

    ZVariable? _variable;
    public ZVariable Variable
    {
        get
        {
            Debug.Assert(Type == OperandTypes.Variable);
            if (Type != OperandTypes.Variable)
            {
                throw new InvalidOperationException("Operand is not a variable");
            }
            if (_variable == null)
            {
                throw new InvalidOperationException("Operand's Variable property has not been initialized");
            }
            return _variable;
        }
        set
        {
            Debug.Assert(Type == OperandTypes.Variable);
            _variable = value;
        }
    }

    public uint Constant { get; set; }

    public ZOperand(OperandTypes type)
    {
        Type = type;
    }

    public int LengthInBytes
    {
        get
        {
            switch (Type)
            {
                case OperandTypes.LargeConstant:
                    return 2;
                case OperandTypes.SmallConstant:
                case OperandTypes.Variable:
                    return 1;
                case OperandTypes.Omitted:
                    return 0;
                default:
                    throw new InvalidOperationException("Unknown Operand Type");
            }
        }
    }
}
