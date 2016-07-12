using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Story
{
    public class ZObject
    {
        // offsets into the byte array to find data items
        private const int ParentIDOffset = 4;
        private const int SiblingIDOffset = 5;
        private const int ChildIDOffset = 6;
        private const int PropertyAddressOffset = 7;
        private const int ZOBJECT_ARRAY_SIZE = 31;

        public const string UNNAMED_OBJECT_NAME = "<unnamed>";
        public const int INVALID_ID = 0;

        private readonly byte[] _bytes;

        public static ZObject InvalidObject = new ZObject(null, 0, ZObject.INVALID_ID);

        public static ushort[] DefaultProperties { get; set; }

        public int BaseAddress { get; }
        public int ID { get; }
        public uint Attributes { get { return _bytes.GetDWord(BaseAddress); } }

        public int ParentID
        {
            get { return _bytes[BaseAddress + ParentIDOffset]; }
            set { _bytes[BaseAddress + ParentIDOffset] = (byte)value; }
        }
        public int SiblingID
        {
            get { return _bytes[BaseAddress + SiblingIDOffset]; }
            set { _bytes[BaseAddress + SiblingIDOffset] = (byte)value; }
        }
        public int ChildID
        {
            get { return _bytes[BaseAddress + ChildIDOffset]; }
            set { _bytes[BaseAddress + ChildIDOffset] = (byte)value; }
        }

        public int PropertyAddress { get { return (int)_bytes.GetWord(BaseAddress + PropertyAddressOffset); } }

        public override string ToString()
        {
            return $"[{ID:D3}] {ShortName}";
        }

        public string ShortName
        {
            get
            {
                int currentIndex = PropertyAddress;
                int length = _bytes[PropertyAddress];
                currentIndex++;
                if (length > 0)
                {
                    var zb = new ZStringBuilder(_bytes, currentIndex, length);
                    return zb.ToString();
                }
                else
                {
                    return UNNAMED_OBJECT_NAME;
                }
            }
        }

        static ZObject()
        {
            DefaultProperties = new ushort[ZOBJECT_ARRAY_SIZE];
        }

        public ZObject(byte[] bytes, int baseAddress, int ID)
        {
            this.ID = ID;
            _bytes = bytes;
            BaseAddress = baseAddress;
        }
    }
}
