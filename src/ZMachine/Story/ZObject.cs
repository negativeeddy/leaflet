using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public int PropertyTableAddress { get { return (int)_bytes.GetWord(BaseAddress + PropertyAddressOffset); } }

        public override string ToString()
        {
            return $"[{ID:D3}] {ShortName}";
        }

        public string ToFullString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ID}. Attributes: ");
            foreach (var bit in Attributes.GetBits())
            {
                sb.Append((31 - (int)bit).ToString());    // TODO: Is printing wrong or is bit labels backwards?
                sb.Append(',');
            }
            sb.AppendLine();

            sb.AppendLine($"   Parent object:  {ParentID}  Sibling object: {SiblingID}  Child object:  {ChildID}");
            sb.AppendLine($"   Property address: {PropertyTableAddress:x4}");

            sb.AppendLine($"       Description: \"{this.ShortName}\"");
            sb.AppendLine("        Properties:");
            foreach (var prop in CustomProperties)
            {
                sb.Append($"        [{prop.ID}] ");
                foreach (byte b in prop.Data)
                {
                    sb.Append($"{b:x2}  ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public int ShortNameLengthInBytes
        {
            get { return (int)_bytes[PropertyTableAddress]; }
        }

        public string ShortName
        {
            get
            {
                return NameBuilder?.ToString() ?? UNNAMED_OBJECT_NAME;
            }
        }

        private ZStringBuilder NameBuilder
        {
            get
            {
                int currentIndex = PropertyTableAddress;
                int length = ShortNameLengthInBytes;
                currentIndex++;
                if (length > 0)
                {
                    var zb = new ZStringBuilder(_bytes, currentIndex, length);
                    NameLengthInBytes = zb.LengthInBytes;
                    return zb;
                }
                else
                {
                    return null;
                }
            }
        }
        private int NameLengthInBytes { get; set; }

        public uint GetPropertyValue(int propertyID)
        {
            var prop = CustomProperties.FirstOrDefault(p => p.ID == propertyID);
            if (prop != null)
            {
                var data = prop.Data;

                if (data.Count == 1)
                {
                    return data[0];
                }
                else if (data.Count == 2)
                {
                    return data.GetWord(0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot get property value. It has {data.Count} bytes");
                }
            }
            else
            {
                // return default property value
                return DefaultProperties[propertyID];
            }
        }

        public void SetPropertyValue(int propertyID, int value)
        {
            var prop = CustomProperties.FirstOrDefault(p => p.ID == propertyID);
            if (prop != null)
            {
                var data = prop.Data;

                if (data.Count == 1)
                {
                    data[0] = (byte)value;
                }
                else if (data.Count == 2)
                {
                    data.SetWord((ushort)value, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot set property value. It has {data.Count} bytes");
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot set property value. Object {ID} does not have property {propertyID} bytes");
            }
        }

        public ZObjectProperty[] CustomProperties
        {
            get
            {
                int propertyAddress = PropertyTableAddress + 1 + NameLengthInBytes;
                var properties = new List<ZObjectProperty>();
                var prop = new ZObjectProperty(_bytes, propertyAddress);
                while (prop.ID != 0)
                {
                    properties.Add(prop);
                    propertyAddress += prop.LengthInBytes;
                    prop = new ZObjectProperty(_bytes, propertyAddress);

                }
                return properties.ToArray();
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
}
