using System;
using System.Collections.Generic;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Story
{
    /// <summary>
    /// Implements the ZMachine dictionary from ZSpec 1.0 section 13.1, 
    /// </summary>
    public class ZDictionary
    {
        private readonly byte _entryLength;
        private readonly byte[] _bytes;
        private readonly int _baseAddress;
        private readonly int _entryBaseAddress;

        public char[] Separators { get; private set; }

        public string[] Words { get; private set; }

        public ZDictionary(byte[] bytes, int address)
        {
            _baseAddress = address;
            _bytes = bytes;

            // Load the dictionary from the byte array at the specified byte address
            int currentAddress = _baseAddress;

            byte numberOfSeparators = _bytes[currentAddress];
            currentAddress++;

            Separators = LoadWordSeparators(_bytes, numberOfSeparators, currentAddress);
            currentAddress += numberOfSeparators;

            _entryLength = _bytes[currentAddress];
            currentAddress++;

            int _entryCount = _bytes.GetWord(currentAddress);
            currentAddress += 2;

            _entryBaseAddress = currentAddress;
            ArraySegment<byte> entryBytes = new ArraySegment<byte>(_bytes, _entryBaseAddress, _entryLength * _entryCount);
            Words = LoadEntries(entryBytes, _entryCount, _entryLength);

            static char[] LoadWordSeparators(IList<byte> data, int count, int address)
            {
                var separators = new char[count];

                for (int i = 0; i < count; i++)
                {
                    separators[i] = (char)data[address + i];
                }
                return separators;
            }

            static string[] LoadEntries(IList<byte> bytes, int count, int length)
            {
                var words = new string[count];

                for (int i = 0; i < count; i++)
                {
                    ZStringBuilder zb = new ZStringBuilder();
                    zb.AddWord(bytes.GetWord(i * length));
                    zb.AddWord(bytes.GetWord((i * length) + 2));
                    words[i] = zb.ToString();
                }
                return words;
            }
        }

        private int IndexOf(string word)
        {
            if (word.Length > 6)
            {
                word = word.Substring(0, 6);
            }

            int idx = Words.IndexOf(word);
            if (idx == -1)
            {
                throw new InvalidOperationException($"Word '{word}' not found in dictionary");
            }
            return idx;
        }

        internal int AddressOf(string word)
        {
            int idx = IndexOf(word);
            return idx * _entryLength + _entryBaseAddress;
        }
    }
}
