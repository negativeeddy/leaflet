using System.Collections.Generic;
using System.Diagnostics;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Instructions
{
    public class BranchOffset
    {
        private IList<byte> _bytes;

        public BranchOffset(IList<byte> bytes)
        {
            _bytes = bytes;
            // implements spec 4.7

            Debug.Assert(_bytes.Count == 2);

            if (_bytes[0].FetchBits(BitNumber.Bit_7, 1) == 0)
            {
                WhenTrue = false;
            }
            else
            {
                WhenTrue = true;
            }

            if (_bytes[0].FetchBits(BitNumber.Bit_6, 1) == 1)
            {
                Offset = _bytes[0].FetchBits(BitNumber.Bit_5, 6);
                LengthInBytes = 1;
            }
            else
            {
                Offset = _bytes.GetWord(0).FetchBits(BitNumber.Bit_13, 14);
                LengthInBytes = 2;

                // branch is actually 14-bit number, check for the negative bit
                if ((Offset & 0x2000) == 0x2000)
                {
                    Offset -= 0x4000;
                }
            }

        }

        public int LengthInBytes { get; }

        public bool WhenTrue { get; }

        public int Offset { get; }

        public override string ToString()
        {
            return $"?{(ushort)Offset:x4}";
        }
    }
}
