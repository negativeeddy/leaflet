using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [TestMethod()]
        public void SaveAndLoadInterpreterState()
        {
            Stream state = new MemoryStream();
            string filename = @"GameFiles\minizork.z3";
            state = RunInterpreter(filename, state, new string[0]);
            state = RunInterpreter(filename, state, new string[] { "open mailbox" });
            state = RunInterpreter(filename, state, new string[] { "get leaflet" });
            state = RunInterpreter(filename, state, new string[] { "read leaflet" });
        }

        private Stream RunInterpreter(string gameFile, Stream state, IEnumerable<string> nextInput)
        {
            Interpreter zMachine = new Interpreter();

            zMachine.Input = new InputFeeder(nextInput);
            zMachine.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Off;

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
