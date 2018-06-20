using System;
using System.Collections.Generic;
using System.Diagnostics;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Story
{
    public class ZObjectProperty
    {
        private Memory<byte> _bytes;
        public int BaseAddress { get; }
        public int LengthInBytes { get { return Data.Length + 1; } }
        public ZObjectProperty(Memory<byte> bytes, int baseAddress)
        {
            _bytes = bytes;
            BaseAddress = baseAddress;
        }

        // size byte is arranged as 32 times the number of data bytes minus 
        // one, plus the property number (spec 12.4.1)
        private byte SizeByte { get { return _bytes.Span[BaseAddress]; } }
        public int ID { get { return SizeByte.FetchBits(BitNumber.Bit_4, 5); } }
        public int DataLength { get { return SizeByte.FetchBits(BitNumber.Bit_7, 3) + 1; } }
        public int DataAddress { get { return BaseAddress + 1;} }

        public Memory<byte> Data
        {
            get
            {
                Debug.Assert(DataLength >= 0);
                Debug.Assert(DataLength <= 8);
                return _bytes.Slice(DataAddress, DataLength);
            }
        }
    }
}