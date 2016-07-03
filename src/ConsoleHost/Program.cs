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

            var dictionary = zm.MainMemory.Dictionary;

            // mini zork's Entrys are (in order) a full stop, a comma and a double-quote
            var expectedEntrys = new string[0];
            var actualEntrys = zm.MainMemory.Dictionary.Words;

            foreach(var item in actualEntrys)
            {
                Console.Write(item);
                Console.Write(" ");
            }

        }
    }
}
