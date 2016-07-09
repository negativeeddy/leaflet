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

            string output = zm.MainMemory.ObjectTree.DumpObjectTree();
            Console.WriteLine($"Object tree contains {zm.MainMemory.ObjectTree.Objects.Count} objects");

            Routine routine = new Routine(zm.MainMemory.Bytes, 0x3b36);

            foreach (var item in routine.Locals.Select((l, i) => new { l, i }))
            {
                Console.WriteLine($"local{item.i}=0x{item.l:x4}");
            }
        }
    }
}
