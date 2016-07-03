using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZMachine.Files;
using ZMachine.Memory;

namespace ZMachine
{
    public class ZMemory
    {
        private byte[] _data;

        public ZMemory(Stream gameMemory)
        {
            _data = new byte[gameMemory.Length];
            gameMemory.Read(_data, 0, _data.Length);

            Header = new ZHeader(_data);
        }

        public ZHeader Header { get; }
        public object Dictionary { get; set; }
        public object ObjectTree { get; set; }

        public IEnumerable<int> AbbreviationTable()
        {
            ushort address = Header.AbbreviationsTableAddress;
            for (int i = 0; i < 96; i++)
            {
                ushort abbrev = _data.GetWord(address);
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
        public string [] TextAbbreviations
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
            ZStringBuilder fragment = new ZStringBuilder(useAbbreviations ? TextAbbreviations: null);
            do
            {
                ushort data = _data.GetWord(address);
                fragment.AddWord(data);
                address += 2;
            }
            while (!fragment.EOS);

            // convert the bytes to characters
            return fragment.ToString();
        }
    }
}