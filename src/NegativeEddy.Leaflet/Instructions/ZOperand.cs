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

    public int LengthInBytes => Type switch
    {
        OperandTypes.LargeConstant => 2,
        OperandTypes.SmallConstant or OperandTypes.Variable => 1,
        OperandTypes.Omitted => 0,
        _ => throw new InvalidOperationException("Unknown Operand Type"),
    };
}
