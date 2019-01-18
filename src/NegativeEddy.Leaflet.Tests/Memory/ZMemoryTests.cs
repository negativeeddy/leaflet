using Microsoft.VisualStudio.TestTools.UnitTesting;
using NegativeEddy.Leaflet.Core.Memory;
using NegativeEddy.Leaflet.TestHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NegativeEddy.Leaflet.Tests.Memory
{
    [TestClass]
    public class ZMemoryTests
    {
        // load a script but execute every save/load on every step
        [TestMethod()]
        public void SaveAndLoadInterpreterState()
        {
            string filename = @"GameFiles\minizork.z3";

            var commands = File.ReadLines(@"GameFiles\miniZork_input_walkthrough.txt");

            Stream state = new MemoryStream();

            // first run initializes the machine
            state = RunInterpreter(filename, state, new string[0]);

            // feed all the scripted commands
            foreach (string command in commands)
            {
                state = RunInterpreter(filename, state, new string[] { command });
            }
        }

        private Stream RunInterpreter(string gameFile, Stream state, IEnumerable<string> nextInput)
        {
            Interpreter zMachine = new Interpreter();

            zMachine.Input = new InputFeeder(nextInput);
            zMachine.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Off;

            var randData = new ShimRandomNumberGenerator();
            zMachine.RandomNumberGenerator = (IRandomNumberGenerator)randData;
            randData.NextRandomNumberToGenerate = 1;

            if (nextInput?.Count() == 0)
            {
                // reset the state of the vm
                using (var stream = System.IO.File.OpenRead(gameFile))
                {
                    zMachine.LoadStory(stream);
                }
            }
            else
            {
                zMachine.ReadState(state);
            }

            using (zMachine.Output.Subscribe(x => Trace.Write(x)))
            {
                // run the machine until it attempts to read from the input
                zMachine.ExecuteCurrentInstruction();

                // run the machine until it attempts to read from the input
                while (zMachine.CurrentInstruction.Definition.Name != "sread")
                {
                    zMachine.ExecuteCurrentInstruction();
                }
            }

            MemoryStream newState = new MemoryStream();
            zMachine.WriteState(newState);
            newState.Seek(0, SeekOrigin.Begin);
            return newState;
        }
    }
}
