using Microsoft.VisualStudio.TestTools.UnitTesting;
using NegativeEddy.Leaflet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegativeEddy.Leaflet.Tests.Story
{
    [TestClass]
    public class InputTests
    {
        [TestMethod]
        public void InputSplitterTest()
        {
            string input = "fred, go fishing";
            string[] expectedOutput = new string[] { "fred", ",", "go", "fishing" };
            string[] actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "open mailbox";
            expectedOutput = new string[] { "open", "mailbox" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "open   mailbox";
            expectedOutput = new string[] { "open", "mailbox" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "onething";
            expectedOutput = new string[] { "onething" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "light\r\n";
            expectedOutput = new string[] { "light" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = " light\r\n";
            expectedOutput = new string[] { "light" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "light ";
            expectedOutput = new string[] { "light" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = " light ";
            expectedOutput = new string[] { "light" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "this has six words in it";
            expectedOutput = new string[] { "this", "has", "six", "words", "in", "it" };
            actualOutput = Interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);
        }

        private static void CompareStringArrays(string[] expectedOutput, string[] actualOutput)
        {
            Assert.AreEqual(expectedOutput.Length, actualOutput.Length, "Counts don't match");
            for (int i = 0; i < expectedOutput.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], actualOutput[i]);
            }
        }
    }
}
