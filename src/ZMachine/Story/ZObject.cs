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
        private readonly byte[] _bytes;

        public const string UNNAMED_OBJECT_NAME = "<unnamed>";
        public const int INVALID_ID = 0;
        public static ZObject InvalidObject = new ZObject(null, 0, ZObject.INVALID_ID);

        public static ushort[] DefaultProperties { get; set; }

        public int BaseAddress { get; }
        public int ID { get; }
        public uint Attributes { get { return _bytes.GetDWord(BaseAddress); } }
        public int ParentID { get { return _bytes[BaseAddress + 4]; } }
        public int SiblingID { get { return _bytes[BaseAddress + 5]; } }
        public int ChildID { get { return _bytes[BaseAddress + 6]; } }
        public int PropertyAddress { get { return (int)_bytes.GetWord(BaseAddress + 7); } }

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
            DefaultProperties = new ushort[31];
        }

        public ZObject(byte[] bytes, int baseAddress, int ID)
        {
            this.ID = ID;
            _bytes = bytes;
            BaseAddress = baseAddress;
        }
    }
}
