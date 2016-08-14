using System;
using System.Diagnostics;
using System.IO;

namespace NegativeEddy.Leaflet.CoreConsoleHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var zm = new Interpreter();
            zm.Input = new ConsoleInput();
            zm.Output.Subscribe(x => Console.Write(x));
            zm.Diagnostics.Subscribe(x => Debug.Write(x));
            zm.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Verbose;

            string filename = Path.Combine("GameFiles", "minizork.z3");
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

            //DumpObjectTree(zm);

            //DumpObjects(zm);

            RunGame(zm);
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
