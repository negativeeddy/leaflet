using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine
{
    public class ZMachine
    {
        public void LoadStory(Stream storyStream)
        {
            MainMemory = new ZMemory(storyStream);
            Stack = new ZStack();
            ProgramCounter = MainMemory.Header.PCStart;
        }
        
        public uint ProgramCounter { get; set; }
        public ZMemory MainMemory { get; set; }

        public ZProcessor Processor { get; set; }
        public ZStack Stack { get; set; }
    }
}
