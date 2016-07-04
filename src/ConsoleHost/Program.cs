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

            var objTree = zm.MainMemory.ObjectTree.Objects;

            foreach (var obj in objTree.Where(zo => zo.ParentID == 0))
            {
                PrintObject(obj, objTree, 0);
            }
        }

        private static void PrintObject(ZObject current, List<ZObject> objects, int level)
        {
            for (int i = 0; i < level; i++) { Console.Write("-----"); }    // do the indention

            Console.WriteLine(current);

            if (current.ChildID != 0)
            {
                PrintObject(objects[current.ChildID - 1], objects, level + 1);
            }

            if (current.SiblingID != 0)
            {
                PrintObject(objects[current.SiblingID - 1], objects, level);
            }
        }
    }
}
