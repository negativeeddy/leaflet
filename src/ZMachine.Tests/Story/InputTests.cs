using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Tests.Story
{
    [TestClass]
    public class InputTests
    {
        [TestMethod]
        public void InputSplitterTest()
        {
            Interpreter interpreter = new Interpreter();

            string input = "fred, go fishing";
            string[] expectedOutput = new string[] { "fred", ",", "go", "fishing" };
            string[] actualOutput = interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "open mailbox";
            expectedOutput = new string[] { "open", "mailbox" };
            actualOutput = interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "onething";
            expectedOutput = new string[] { "onething" };
            actualOutput = interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);

            input = "this has six words in it";
            expectedOutput = new string[] { "this", "has", "six", "words", "in", "it" };
            actualOutput = interpreter.SplitInput(input).ToArray();
            CompareStringArrays(expectedOutput, actualOutput);
        }

        private void CompareStringArrays(string[] expectedOutput, string[] actualOutput)
        {
            Assert.AreEqual(expectedOutput.Length, actualOutput.Length, "Counts don't match");
            for (int i = 0; i < expectedOutput.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], actualOutput[i]);
            }
        }
    }
}
