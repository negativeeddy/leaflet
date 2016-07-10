using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Memory
{
    public class WordOverByteOverlay : IList<ushort>
    {
        private IList<byte> _bytes;
        private int _baseAddress;
        private int _length;

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public WordOverByteOverlay(IList<byte> bytes, int baseAddress, int length)
        {
            Debug.Assert(bytes.Count >= baseAddress + length * 2);
            _bytes = bytes;
            _baseAddress = baseAddress;
            _length = length;
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
            for(int i = 0; i<_length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for(int i=0; i<_length; i++)
            {
                yield return this[i];
            }
        }
    }
}
