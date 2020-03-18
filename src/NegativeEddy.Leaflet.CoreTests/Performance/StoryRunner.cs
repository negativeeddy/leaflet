using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NegativeEddy.Leaflet.Core.Memory;
using NegativeEddy.Leaflet.Story;
using NegativeEddy.Leaflet.TestHelpers;
using BenchmarkDotNet.Attributes;

namespace NegativeEddy.Leaflet.Performance.Tests
{
    public class StoryRunner
    {
        private readonly Interpreter _zm;
        private readonly InputFeeder _feeder;
        public StoryRunner(string gameFile, string inputFile)
        {
            _zm = new Interpreter
            {
                Input = _feeder = new InputFeeder(File.ReadLines(inputFile), false),
                DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Off,
            };

            _zm.Output.Subscribe(x => Console.Write(x));
            _zm.Diagnostics.Subscribe(x => Debug.Write(x));

            using (var stream = File.OpenRead(gameFile))
            {
                _zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {_zm.MainMemory.Header.Version}");
            RunGame();
        }

        [Benchmark]
        public void RunGame()
        {
            while (_zm.IsRunning && _feeder.InputAvailable)
            {
                _zm.ExecuteCurrentInstruction();
            }
        }
    }
}