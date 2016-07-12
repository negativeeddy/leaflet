using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZMachine.Memory;

namespace ZMachine.Story
{
    public class ZObjectProperty
    {
        public int LengthInBytes { get { return Data.Count + 1; } }
        public IList<byte> Data { get; }
        public int ID { get; }
        public ZObjectProperty(IList<byte> bytes, int baseAddress)
        {
            byte sizeByte = bytes[baseAddress];

            // size byte is arranged as 32 times the number of data bytes minus 
            // one, plus the property number (spec 12.4.1)
            int dataLength = sizeByte.FetchBits(BitNumber.Bit_7, 3) + 1;
            ID = sizeByte.FetchBits(BitNumber.Bit_4, 5);
            //lengthbyte = (32 * (LengthInBytes - 1)) + ID;
            Debug.Assert(dataLength >= 0);
            Debug.Assert(dataLength <= 8);
            Data = bytes.Skip(baseAddress + 1).Take(dataLength).ToArray();
        }
    }
}