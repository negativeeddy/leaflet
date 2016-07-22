using System;
using System.Collections.Generic;
using ZMachine.Memory;

namespace ZMachine.Story
{
    /// <summary>
    /// Implements the ZMachine dictionary from ZSpec 1.0 section 13.1, 
    /// </summary>
    public class ZDictionary
    {
        private char[] _separators;
        private byte _entryLength;
        private byte[] _bytes;
        private int _baseAddress;
        private int _entryBaseAddress;

        public char[] Separators
        {
            get { return _separators; }
            private set { _separators = value; }
        }

        public string[] Words { get; private set; }

        public ZDictionary(byte[] bytes, int address)
        {
            _baseAddress = address;
            _bytes = bytes;
            Load();
        }

        public ZDictionary() : base()
        {
        }

        /// <summary>
        /// Loads the dictionary from the byte array at the specified byte address
        /// </summary>
        /// <param name="data">an array of bytes</param>
        /// <param name="baseAddress">the index of the beginning of the dictionary in the data array</param>
        private void Load()
        {
            int currentAddress = _baseAddress;
            byte numberOfSeparators = _bytes[currentAddress];
            currentAddress++;

            LoadWordSeparators(_bytes, numberOfSeparators, currentAddress);
            currentAddress += numberOfSeparators;

            _entryLength = _bytes[currentAddress];
            currentAddress++;

            int _entryCount = _bytes.GetWord(currentAddress);
            currentAddress += 2;

            _entryBaseAddress = currentAddress;
            ArraySegment<byte> entryBytes = new ArraySegment<byte>(_bytes, _entryBaseAddress, _entryLength * _entryCount);
            LoadEntries(entryBytes, _entryCount, _entryLength);
        }

        private void LoadEntries(IList<byte> bytes, int count, int length)
        {
            Words = new string[count];

            for(int i=0; i<count; i++)
            {
                ZStringBuilder zb = new ZStringBuilder();
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

        internal int IndexOf(string word)
        {
            if (word.Length > 6)
            {
                word = word.Substring(0, 6);
            }
            return Words.IndexOf(word);
        }

        internal int AddressOf(string word)
        {
            return IndexOf(word) * _entryLength + _entryBaseAddress;
        }
    }
}
