namespace NegativeEddy.Leaflet.Memory;

public class PackedAddress
{
    public ushort Bits { get; }

    public PackedAddress(ushort bits)
    {
        Bits = bits;
    }

    int Address
    {
        get { return Bits * 2; }
    }
}
