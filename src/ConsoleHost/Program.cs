using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine;
using ZMachine.Memory;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var zm = new ZMachine.Interpreter();

            string filename = @"GameFiles\minizork.z3";
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

            //string output = zm.MainMemory.ObjectTree.DumpObjectTree();
            //Console.WriteLine(output);
            Console.WriteLine($"Object tree contains {zm.MainMemory.ObjectTree.Objects.Count} objects");

            var objTree = zm.MainMemory.ObjectTree;
            
            foreach(var obj in objTree)
            {
                Console.WriteLine(obj.ToFullString());
            }

            //RunGame(zm);
        }

        private static void RunGame(Interpreter zm)
        {
            bool showFrames = false;
            while (true)
            {
                //zm.Print(showFrames);
                zm.ExecuteCurrentInstruction();
            }
        }
    }
}
