using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            zm.Input = new ConsoleInput();
            zm.Output.Subscribe(x => Console.Write(x));
            zm.Diagnostics.Subscribe(x => Debug.Write(x));

            string filename = @"GameFiles\minizork.z3";
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

            //DumpObjectTree(zm);

            //DumpObjects(zm);

            RunGame(zm);
        }

        private static void DumpObjectTree(Interpreter zm)
        {
            string output = zm.MainMemory.ObjectTree.DumpObjectTree();
            Console.WriteLine(output);
            Console.WriteLine($"Object tree contains {zm.MainMemory.ObjectTree.Objects.Count} objects");
        }

        private static void DumpObjects(Interpreter zm)
        {
            var objTree = zm.MainMemory.ObjectTree;

            foreach (var obj in objTree)
            {
                Console.WriteLine(obj.ToFullString());
            }
        }

        private static void RunGame(Interpreter zm)
        {
            while (true)
            {
                zm.ExecuteCurrentInstruction();
            }
        }
    }
}
