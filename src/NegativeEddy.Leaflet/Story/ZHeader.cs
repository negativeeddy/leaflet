using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Story;

public class ZHeader
{
    //header offsets from spec 11.1
    private const int HeaderOffset_Version = 0x00;
    private const int HeaderOffset_Flags1 = 0x01;
    private const int HeaderOffset_ReleaseNumber = 0x02;
    private const int HeaderOffset_BaseOfHighMemory = 0x04;
    private const int HeaderOffset_InitialValueOfProgramCounter = 0x06;
    private const int HeaderOffset_LocationOfDictionary = 0x08;
    private const int HeaderOffset_LocationOfObjectTable = 0x0A;
    private const int HeaderOffset_LocationOfGlobalVariablesTable = 0x0C;
    private const int HeaderOffset_BaseOfStaticMemory = 0x0E;
    private const int HeaderOffset_Flags2 = 0x10;
    private const int HeaderOffset_SerialCode = 0x12;
    private const int HeaderOffset_LocationOfAbbreviationsTable = 0x18;
    private const int HeaderOffset_Lengthoffile = 0x1A;
    private const int HeaderOffset_ChecksumOfFile = 0x1C;
    private const int HeaderOffset_Interpreternumber = 0x1E;
    private const int HeaderOffset_Interpreterversion = 0x1F;

    private const int HeaderOffset_ScreenheightinLines = 0x20;
    private const int HeaderOffset_ScreenwidthInChars = 0x21;
    private const int HeaderOffset_ScreenwidthInUnits = 0x22;
    private const int HeaderOffset_ScreenheightInUnits = 0x24;
    private const int HeaderOffset_Fontwidth = 0x26;

    private const int HeaderOffset_Fontheight = 0x27;

    private const int HeaderOffset_Routinesoffset = 0x28;
    private const int HeaderOffset_Staticstrings = 0x2A;
    private const int HeaderOffset_Defaultbackgroundcolour = 0x2C;
    private const int HeaderOffset_Defaultforegroundcolour = 0x2D;
    private const int HeaderOffset_Addressofterminatingcharacterstable = 0x2E;
    private const int HeaderOffset_Totalwidth = 0x30;
    private const int HeaderOffset_Standardrevisionnumber = 0x32;
    private const int HeaderOffset_Alphabettableaddress = 0x34;
    private const int HeaderOffset_Headerextensiontableaddress = 0x36;

    private byte[] _data;

    public ZHeader(byte[] _data)
    {
        this._data = _data;
    }

    public byte Version { get { return _data[HeaderOffset_Version]; } }

    public uint Flags1 { get { return _data.GetDWord(HeaderOffset_Flags1); } }
    public int ReleaseNumber { get { return _data.GetWord(HeaderOffset_ReleaseNumber); } }
    public ushort HighMemoryAddress { get { return _data.GetWord(HeaderOffset_BaseOfHighMemory); } }
    public int PCStart { get { return (int)_data.GetWord(HeaderOffset_InitialValueOfProgramCounter); } }
    public ushort DictionaryAddress { get { return _data.GetWord(HeaderOffset_LocationOfDictionary); } }
    public ushort ObjectTableAddress { get { return _data.GetWord(HeaderOffset_LocationOfObjectTable); } }
    public ushort GlobalVariablesTableAddress { get { return _data.GetWord(HeaderOffset_LocationOfGlobalVariablesTable); } }
    public ushort StaticMemoryAddress { get { return _data.GetWord(HeaderOffset_BaseOfStaticMemory); } }
    public uint Flags2 { get { return _data.GetDWord(HeaderOffset_Flags2); } }

    public string SerialCode
    {
        get
        {
            string code = string.Empty;
            int start = HeaderOffset_SerialCode;
            int end = start + 6;
            for (int i = start; i < end; i++)
            {
                code += (char)_data[i];
            }
            return code;
        }
    }

    public ushort AbbreviationsTableAddress { get { return _data.GetWord(HeaderOffset_LocationOfAbbreviationsTable); } }
    public ushort Filelength { get { return _data.GetWord(HeaderOffset_Lengthoffile); } }
    public ushort Checksum { get { return _data.GetWord(HeaderOffset_ChecksumOfFile); } }
    public byte InterpreterNumber { get { return _data[HeaderOffset_Interpreternumber]; } }
    public byte InterpreterVersion { get { return _data[HeaderOffset_Interpreterversion]; } }
    public byte ScreenHeightLines { get { return _data[HeaderOffset_ScreenheightinLines]; } }
    public byte ScreenWidthChars { get { return _data[HeaderOffset_ScreenwidthInChars]; } }
    public ushort ScreenHeightUnits { get { return _data.GetWord(HeaderOffset_ScreenheightInUnits); } }
    public ushort ScreenWidthUnits { get { return _data.GetWord(HeaderOffset_ScreenwidthInUnits); } }

    public byte StandardRevisionNumber { get { return _data[HeaderOffset_Standardrevisionnumber]; } }
}
