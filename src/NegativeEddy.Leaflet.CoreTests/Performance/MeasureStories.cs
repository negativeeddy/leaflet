using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using NegativeEddy.Leaflet.Memory;
using BenchmarkDotNet.Running;
using System.Diagnostics;

namespace NegativeEddy.Leaflet.Performance.Tests
{

    public class ZorkMailboxStory : StoryRunner
    {
        public ZorkMailboxStory() : base(@"GameFiles\minizork.z3", @"GameFiles\miniZork_input_mailbox.txt")
        { }
    }

    public class ZorkWalkthrough : StoryRunner
    {
        public ZorkWalkthrough() : base(@"GameFiles\minizork.z3", @"GameFiles\miniZork_input_walkthrough2.txt")
        { }
    }

    [TestClass()]
    public class MeasureStories
    {
        [Ignore]
        [TestMethod()]
        public void MeasureMiniZork_Mailbox()
        {
            BenchmarkRunner.Run<ZorkMailboxStory>();
        }

        [Ignore]
        [TestMethod()]
        public void MeasureMiniZork_Walkthrough()
        {
            BenchmarkRunner.Run<ZorkWalkthrough>();
        }

        [Ignore]
        [TestMethod()]
        public void TmpMeasureMiniZork_Walkthrough()
        {
            var z = new ZorkWalkthrough();
            var sw = new Stopwatch();
            sw.Start();
            z.RunGame();
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed);
        }
    }
}