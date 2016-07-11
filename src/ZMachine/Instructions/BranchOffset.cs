using System.Collections.Generic;
using System.Diagnostics;
using ZMachine.Memory;

namespace ZMachine.Instructions
{
    public class BranchOffset
    {
        public BranchOffset(IList<byte> _bytes)
        {
            // implements spec 4.7

            Debug.Assert(_bytes.Count == 2);

            byte b = _bytes[0];
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
                Offset = (ushort)((_bytes[0].FetchBits(BitNumber.Bit_5, 6) << 8) | _bytes[1]);
                LengthInBytes = 2;
            }
        }

        public int LengthInBytes { get; }

        public bool WhenTrue { get; }
        public ushort Offset { get; }

        public override string ToString()
        {
            return $"?{(ushort)Offset:x4}";
        }
    }
}
