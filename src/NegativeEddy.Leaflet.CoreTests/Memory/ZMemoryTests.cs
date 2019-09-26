using Microsoft.VisualStudio.TestTools.UnitTesting;
using NegativeEddy.Leaflet.TestHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NegativeEddy.Leaflet.Tests.Memory
{
    [TestClass]
    public class ZMemoryTests
    {
        [TestMethod()]
        public void SaveAndLoadInterpreterState()
        {
            var stateStream = new MemoryStream();
            string filename = @"GameFiles\minizork.z3";
            RunInterpreter(filename, stateStream, new string[0]);
            RunInterpreter(filename, stateStream, new string[] { "open mailbox" });
            RunInterpreter(filename, stateStream, new string[] { "get leaflet" });
            RunInterpreter(filename, stateStream, new string[] { "read leaflet" });
        }

        private void RunInterpreter(string gameFile, MemoryStream stateStream, IEnumerable<string> nextInput)
        {
            Interpreter zMachine = new Interpreter
            {
                Input = new InputFeeder(nextInput),
                DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Off
            };

            if (nextInput.Count() == 0)
            {
                // reset the state of the vm
                using var stream = System.IO.File.OpenRead(gameFile);
                zMachine.LoadStory(stream);
            }
            else
            {
                zMachine.ReadState(stateStream);
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

            stateStream.Seek(0, SeekOrigin.Begin);
            zMachine.WriteState(stateStream);
            stateStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
