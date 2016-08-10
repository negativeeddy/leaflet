using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace NegativeEddy.Leaflet.Memory
{
    /// <summary>
    /// An array which is constructed from a section of a byte array
    /// The array words are 
    /// </summary>
    public class WordOverByteArray : IList<ushort>
    {
        private IList<byte> _bytes;
        private int _baseAddress;

        /// <summary>
        /// Number of words in the array
        /// </summary>
        private int _count;

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public bool IsReadOnly { get { return false; } }

        public WordOverByteArray(IList<byte> bytes, int baseAddress, int length)
        {
            Debug.Assert(bytes.Count >= baseAddress + length * 2);
            _bytes = bytes;
            _baseAddress = baseAddress;
            _count = length;
        }

        public WordOverByteArray(IList<byte> bytes, int baseAddress)
        {
            Debug.Assert(bytes.Count >= _count);
            _count = (bytes.Count - baseAddress) / 2;
            _bytes = bytes;
            _baseAddress = baseAddress;
        }

        public ushort this[int index]
        {
            get
            {
                return _bytes.GetWord(_baseAddress + index * 2);
            }
            set
            {
                _bytes.SetWord(value, _baseAddress + index * 2);
            }
        }

        public int IndexOf(ushort item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ushort item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(ushort item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ushort item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ushort[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ushort item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }
}
