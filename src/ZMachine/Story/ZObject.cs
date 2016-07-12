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
        private uint Attributes
        {
            get { return _bytes.GetDWord(BaseAddress); }
            set { _bytes.SetDWord(value, BaseAddress); }
        }

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

        public string ToFullString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ID}. Attributes: ");
            foreach(var bit in Attributes.GetBits())
            {
                sb.Append(((int)bit).ToString());
                sb.Append(',');
            }
            sb.AppendLine();

            sb.AppendLine($"   Parent object:  {ParentID}  Sibling object: {SiblingID}  Child object:  {ChildID}");
            sb.AppendLine($"   Property address: {PropertyAddress:x4}");

            sb.AppendLine($"       Description: \"{this.ShortName}\"");
            sb.AppendLine("        Properties:");
            //  [23] 64 ae
            //[22] 77
            //[18] 44 09
            //[12] 4c 01
            return sb.ToString();
        }

        ObjectProperty[] Properties
        {
            get
            {
                var p = new ObjectProperty(new ArraySegment<byte>(_bytes, PropertyAddress, 100));
                return new ObjectProperty[] { p };
            }
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

        /// <summary>
        /// Checks if the object has a specific attribute enabled
        /// </summary>
        /// <param name="attributeNumber">the bit of the attribute to check (0-31)</param>
        /// <returns>true if the object has the atttribute set</returns>
        internal bool HasAttribute(BitNumber attributeNumber)
        {

            return Attributes.FetchBits(attributeNumber, 1) == 1;
        }

        /// <summary>
        /// Sets or clears an attribute on an object
        /// </summary>
        /// <param name="attributeNumber">the bit of the attribute to change (0-31)</param>
        /// <param name="set">if true, the attribute is set, else the attribute is cleared</param>
        internal void SetAttribute(BitNumber attributeNumber, bool set)
        {
                Attributes = Attributes.SetBit(attributeNumber, set);
        }
    }

    class ObjectProperty
    {
        public int Size;
        public int LengthInBytes{ get; }
        public byte[] Data { get; }

        public ObjectProperty(IList<byte> bytes)
        {
            byte sizeByte = bytes[0];
            Console.WriteLine(sizeByte.ToString("x4"));
            if (sizeByte == 0)
            {
                return;
            }
        }
    }
}
