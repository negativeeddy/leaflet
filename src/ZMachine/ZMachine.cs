using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine
{
    public class ZMachine
    {
        public void LoadStory(Stream storyStream)
        {
            MainMemory = new ZMemory(storyStream);
            Stack = new Stack<Routine>();
            ProgramCounter = MainMemory.Header.PCStart;
        }
        
        public uint ProgramCounter { get; set; }
        public ZMemory MainMemory { get; set; }

        public ZProcessor Processor { get; set; }
        public Stack<Routine> Stack { get; set; }
    }
}
