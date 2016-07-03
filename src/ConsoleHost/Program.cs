using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var zm = new ZMachine.ZMachine();

            string filename = @"GameFiles\minizork.z3";
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

            var objTree = zm.MainMemory.ObjectTree;

            // mini zork's Entrys are (in order) a full stop, a comma and a double-quote
            int expectedEntrysCount = 1152;
            int actualEntrysCount = zm.MainMemory.ObjectTree.Objects.Count();

            Console.WriteLine(actualEntrysCount);

            foreach(var o in zm.MainMemory.ObjectTree.Objects)
            {
                Console.WriteLine($"{o.ID} (0x{o.BaseAddress:X4}) => {o.ShortName}\n   Prop:0x{o.PropertyAddress:X4}");
            }
            //foreach(var item in actualEntrys)
            //{
            //    Console.Write(item);
            //    Console.Write(" ");
            //}

        }
    }
}
