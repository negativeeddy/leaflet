using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Story
{
    /// <summary>
    /// Implements the ZMachine dictionary from ZSpec 1.0 section 13.1, 
    /// </summary>
    public class ZDictionary
    {
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

            Load(memory, address);
        }

        public ZDictionary() : base()
        {
        }

        /// <summary>
        /// Loads the dictionary from the byte array at the specified byte address
        /// </summary>
        /// <param name="data">an array of bytes</param>
        /// <param name="baseAddress">the index of the beginning of the dictionary in the data array</param>
        private void Load(byte[] data, int baseAddress)
        {
            int currentAddress = baseAddress;
            byte numberOfSeparators = data[currentAddress];
            currentAddress++;

            LoadWordSeparators(data, numberOfSeparators, currentAddress);
            currentAddress += numberOfSeparators;

            int _entryLength = data[currentAddress];
            currentAddress++;

            int _entryCount = data.GetWord(currentAddress);
            currentAddress += 2;

            int _entryBaseAddress = currentAddress;
            ArraySegment<byte> entryBytes = new ArraySegment<byte>(data, _entryBaseAddress, _entryLength * _entryCount);
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
    }
}
