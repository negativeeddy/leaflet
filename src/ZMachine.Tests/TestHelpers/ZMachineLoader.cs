using NegativeEddy.Leaflet;
using System;
using System.IO;

namespace NegativeEddy.Leaflet.TestHelpers
{
    public class ZMachineLoader
    {
        static public Interpreter Load(string filename)
        {
            Console.WriteLine("Loading " + filename);
            var zm = new Interpreter();
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine("Loaded " + filename);

            Console.WriteLine($"ZMachine Version {zm.MainMemory.Header.Version}");
            return zm;
        }
    }
}
