using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NegativeEddy.Leaflet.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var zm = new Interpreter();
            zm.Input = new ConsoleInput();
            zm.Output.Subscribe(x => Console.Write(x));
            zm.Diagnostics.Subscribe(x => Debug.Write(x));
            zm.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Verbose;

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
            while (zm.IsRunning)
            {
                zm.ExecuteCurrentInstruction();
            }
        }
    }
}
