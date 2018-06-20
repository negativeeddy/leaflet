using NegativeEddy.Leaflet.Story;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NegativeEddy.Leaflet.Memory
{
    public class ZMemory
    {
        private byte[] _bytes;

        public Memory<byte> Bytes => new Memory<byte>(_bytes);

        public ZMemory(byte[] gameMemory)
        {
            byte[] tmp = new byte[gameMemory.Length];
            gameMemory.CopyTo(tmp, 0);
            _bytes = tmp;

            // update the global zstringbuilder table with the text abbreviations
            ZStringBuilder.AbbreviationTable = TextAbbreviations;
        }

        public ZMemory(Stream gameMemory)
        {
            _bytes = new byte[gameMemory.Length];
            gameMemory.Read(_bytes, 0, Bytes.Length);

            // update the global zstringbuilder table with the text abbreviations
            ZStringBuilder.AbbreviationTable = TextAbbreviations;
        }

        public ZHeader Header => new ZHeader(Bytes);

        public ReadOnlyMemory<byte> StaticMemory
        {
            get { return Bytes.Slice(Header.StaticMemoryAddress, Bytes.Length - Header.StaticMemoryAddress); }
        }

        public Memory<byte> DynamicMemory
        {
            get { return Bytes.Slice(0, Header.StaticMemoryAddress); }
        }

        public Memory<byte> HighMemory
        {
            get { return Bytes.Slice(Header.HighMemoryAddress, Bytes.Length - Header.HighMemoryAddress); }
        }

        private ZDictionary _dictionary;

        public ZDictionary Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new ZDictionary(Bytes, Header.DictionaryAddress);
                }
                return _dictionary;
            }
        }

        private ZObjectTable _objectTree;
        public ZObjectTable ObjectTree
        {
            get
            {
                if (_objectTree == null)
                {
                    _objectTree = new ZObjectTable(Bytes, Header.ObjectTableAddress);
                }
                return _objectTree;
            }
        }

        public IEnumerable<int> AbbreviationTable()
        {
            ushort address = Header.AbbreviationsTableAddress;
            for (int i = 0; i < 96; i++)
            {
                ushort abbrev = Bytes.GetWord(address);
                int final = abbrev.ToWordZStringAddress();
                yield return final;

                address += 2;   // incremenet 2 bytes (1 word)
            }
        }

        public string GetTextAbbreviation(int index)
        {

            int address = AbbreviationTable().Skip(index).First();
            return ReadString(address);
        }

        string[] _textAbbreviations = null;
        public string[] TextAbbreviations
        {
            get
            {
                if (_textAbbreviations == null)
                {
                    _textAbbreviations = AbbreviationTable().Select(addr => ReadString(addr, false)).ToArray();
                }
                return _textAbbreviations;
            }
        }
        public string ReadString(int address, bool useAbbreviations = true)
        {
            // load all the fragments until reaching the end of the string
            ZStringBuilder fragment = new ZStringBuilder();
            do
            {
                ushort data = Bytes.GetWord(address);
                fragment.AddWord(data);
                address += 2;
            }
            while (!fragment.EOS);

            // convert the bytes to characters
            return fragment.ToString();
        }

        public WordOverByteArray GlobalVariables
        {
            get
            {
                return new WordOverByteArray(Bytes, Header.GlobalVariablesTableAddress, 255 - 16);
            }
        }
    }
}