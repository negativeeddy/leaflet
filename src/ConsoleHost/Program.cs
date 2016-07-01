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
            Console.WriteLine($"Version {zm.MainMemory.Header.Version}");

            var zb = new ZStringBuilder();
            var test = zm.MainMemory.ReadString(0xb106);
            Console.WriteLine(test);
        }
    }
}
