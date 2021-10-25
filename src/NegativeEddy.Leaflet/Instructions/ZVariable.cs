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
        return Location switch
        {
            ZVariableLocation.Global => "g" + Value.ToString("x"),
            ZVariableLocation.Local => "local" + Value.ToString("x"),
            ZVariableLocation.Stack => "sp",
            _ => throw new InvalidOperationException($"Unknown variable location '{Location}'"),
        };
    }

    public string ToInfoDumpFormat(bool popIfStack = true)
    {
        return Location switch
        {
            ZVariableLocation.Global => "G" + Value.ToString("x2"),
            ZVariableLocation.Local => "L" + Value.ToString("x2"),
            ZVariableLocation.Stack => (popIfStack ? '-' : '+') + "(SP)",
            _ => throw new InvalidOperationException($"Unknown variable location '{Location}'"),
        };
    }
}
