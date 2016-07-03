using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine
{
    /// <summary>
    /// Implements the ZMachine dictionary from ZSpec 1.0 section 13.1, 
    /// </summary>
    public class ZDictionary
    {
        private byte[] _data;
        private int _baseAddress;
        private int _entryBaseAddress;
        private byte _entryLength;

        private char[] _separators;

        public char[] Separators
        {
            get { return _separators; }
            private set { _separators = value; }
        }

        private string[] _words;

        public string[] Words
        {
            get { return _words; }
            private set { _words = value; }
        }

        public ZDictionary(byte[] memory, int address)
        {
            _data = memory;
            _baseAddress = address;

            Load();
        }

        public ZDictionary() : base()
        {
        }

        private void Load()
        {
            int currentAddress = _baseAddress;
            byte numberOfSeparators = _data[currentAddress];
            currentAddress++;

            LoadWordSeparators(_data, numberOfSeparators, currentAddress);
            currentAddress += numberOfSeparators;

            int _entryLength = _data[currentAddress];
            currentAddress++;

            int _entryCount = _data.GetWord(currentAddress);
            currentAddress += 2;

            int _entryBaseAddress = currentAddress;
            ArraySegment<byte> entryBytes = new ArraySegment<byte>(_data, _entryBaseAddress, _entryLength * _entryCount);
            LoadEntries(entryBytes, _entryCount, _entryLength);
        }

        private void LoadEntries(IList<byte> bytes, int count, int length)
        {
            Words = new string[count];

            for(int i=0; i<count; i++)
            {
                ZStringBuilder zb = new ZStringBuilder(null);
                zb.AddWord(bytes.GetWord(i*length));
                zb.AddWord(bytes.GetWord((i*length)+2));
                Words[i] = zb.ToString();
            }
        }

        private void LoadWordSeparators(IList<byte> data, int count, int address)
        {
            Separators = new char[count];

            for (int i = 0; i < count; i++)
            {
                Separators[i] = (char)data[address + i];
            }
        }
    }

    public class DictionaryEntry
    {
        public DictionaryEntry(IList<byte> bytes, int textSize = 4)
        {
            ZStringBuilder zb = new ZStringBuilder(null);
            for (int i = 0; i < textSize/2; i++) 
            {
                var b = bytes.GetWord(i);
                zb.AddWord(b);
            }
            string text = zb.ToString();
        }
    }
}
