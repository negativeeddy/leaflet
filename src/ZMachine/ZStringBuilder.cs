using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine
{
    public class ZStringBuilder
    {
        IEnumerable<string> _abbrevTable;

        public ZStringBuilder(IEnumerable<string> abbreviationTable)
        {
            _abbrevTable = abbreviationTable;
        }

        private List<ushort> _allBits;

        public void AddWord(ushort bits)
        {
            if (_allBits == null)
            {
                _allBits = new List<ushort>();
            }
            _allBits.Add(bits);
        }

        public bool EOS
        {
            get
            {
                if (_allBits.Count == 0)
                {
                    return false;
                }
                ushort upperBit = _allBits.Last().FetchBits(BitNumber.Bit_15, 1);
                return upperBit == 1;
            }
        }

        private static char[][] alphabet_tables = new char[][] {
            new char [] {
            ' ', '?', '?', '?', '?', '?', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'  },
            new char [] {
            ' ', '?', '?', '?', '?', '?', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'  },
            new char [] {
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', '\n', '0', '1', '2', '3', '4', '5', '6', '7', '8',
            '9', '.', ',', '!', '?', '_', '#', '\'', '"', '/', '\\', '-', ':', '(', ')',     },
        };

        private IEnumerable<byte> BytesFromBits()
        {
            foreach (var bits in _allBits)
            {
                yield return (byte)bits.FetchBits(BitNumber.Bit_14, 5);
                yield return (byte)bits.FetchBits(BitNumber.Bit_9, 5);
                yield return (byte)bits.FetchBits(BitNumber.Bit_4, 5);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int currentAlphabet = 0;
            int? abbrevAddrZ = null;
            int? abbrevAddrX = null;

            foreach (byte b in BytesFromBits())
            {
                if (abbrevAddrZ != null)
                {
                    abbrevAddrX = b;
                    int abbrevAddr = 32 * (abbrevAddrZ.Value - 1) + abbrevAddrX.Value;
                    string abbrev = GetAbbreviation(abbrevAddr);
                    abbrevAddrZ = null;
                    abbrevAddrX = null;
                    sb.Append(abbrev);
                }
                else
                {

                    switch (b)
                    {
                        case 1:
                        case 2:
                        case 3:
                            abbrevAddrZ = b;
                            break;
                        case 4:
                            currentAlphabet = 1;
                            break;
                        case 5:
                            currentAlphabet = 2;
                            break;
                        case 6:
                            if (currentAlphabet == 2)
                            {
                                // 6 is not a special case unless on alphabet 2
                                throw new NotImplementedException("ASCII string encoding not implemented yet. See z-spec 3.4");
                            }
                            else
                            { 
                                sb.Append(alphabet_tables[currentAlphabet][b]);
                                currentAlphabet = 0;
                            }
                            break;
                        default:
                            sb.Append(alphabet_tables[currentAlphabet][b]);
                            currentAlphabet = 0;
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        private string GetAbbreviation(int abbrevAddr)
        {
            Debug.Assert(abbrevAddr < 96);

            if (_abbrevTable == null)
            {
                return "ABBREV_UKNOWN";
            }
            else
            {
                return _abbrevTable.Skip(abbrevAddr).First();
            }
        }
    }
}
