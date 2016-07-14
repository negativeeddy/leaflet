using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZMachine.Memory;

namespace ZMachine.Story
{
    public class ZObjectProperty
    {
        private IList<byte> _bytes;
        public int BaseAddress { get; }
        public int LengthInBytes { get { return Data.Count + 1; } }
        public ZObjectProperty(IList<byte> bytes, int baseAddress)
        {
            _bytes = bytes;
            BaseAddress = baseAddress;
        }

        // size byte is arranged as 32 times the number of data bytes minus 
        // one, plus the property number (spec 12.4.1)
        private byte SizeByte { get { return _bytes[BaseAddress]; } }
        public int ID { get { return SizeByte.FetchBits(BitNumber.Bit_4, 5); } }
        private int DataLength { get { return SizeByte.FetchBits(BitNumber.Bit_7, 3) + 1; } }
        public IList<byte> Data
        {
            get
            {
                Debug.Assert(DataLength >= 0);
                Debug.Assert(DataLength <= 8);
                return new ArraySegment<byte>((byte[])_bytes, BaseAddress + 1, DataLength);
            }
        }
    }
}