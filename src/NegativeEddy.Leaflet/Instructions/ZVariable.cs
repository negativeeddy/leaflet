namespace NegativeEddy.Leaflet.Instructions;

public enum ZVariableLocation { Stack, Local, Global }

[Serializable]
public class ZVariable
{
    /// <summary>
    /// the original bits of the variable type of the operand
    /// </summary>
    public byte Bits { get; }

    public ZVariable(byte bits)
    {
        Bits = bits;
    }

    public int Value
    {
        get
        {
            if (Bits >= 0x10)
            {
                return Bits - 0x10;
            }
            else if (Bits >= 0x01)
            {
                return Bits - 0x01;
            }
            else
            {
                return 0;
            }
        }
    }

    public ZVariableLocation Location
    {
        get
        {
            if (Bits >= 0x10)
            {
                return ZVariableLocation.Global;
            }
            else if (Bits >= 0x01)
            {
                return ZVariableLocation.Local;
            }
            else
            {
                return ZVariableLocation.Stack;
            }
        }
    }

    public override string ToString()
    {
        switch (Location)
        {
            case ZVariableLocation.Global:
                return "g" + Value.ToString("x");
            case ZVariableLocation.Local:
                return "local" + Value.ToString("x");
            case ZVariableLocation.Stack:
                return "sp";
            default:
                throw new InvalidOperationException($"Unknown variable location '{Location}'");
        }
    }

    public string ToInfoDumpFormat(bool popIfStack = true)
    {
        switch (Location)
        {
            case ZVariableLocation.Global:
                return "G" + Value.ToString("x2");
            case ZVariableLocation.Local:
                return "L" + Value.ToString("x2");
            case ZVariableLocation.Stack:
                return (popIfStack ? '-' : '+') + "(SP)";
            default:
                throw new InvalidOperationException($"Unknown variable location '{Location}'");
        }
    }
}
