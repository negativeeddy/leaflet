using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Tests
{
    [TestClass]
    public class StringBuilderTests
    {
        [TestMethod]
        public void AbbreviationExpansionTest()
        {
            string expected = "\"Flood Control Dam #3 was constructed in 783 GUE with a grant of 37 million zorkmids from Lord Dimwit Flathead the Excessive. This impressive structure is composed of 370,000 cubic feet of concrete, is 256 feet tall and 193 feet wide.";

            var zm = new ZMachine();

            string filename = @"GameFiles\minizork.z3";
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Version {zm.MainMemory.Header.Version}");

            var zb = new ZStringBuilder(zm.MainMemory.TextAbbreviations);
            string actual = zm.MainMemory.ReadString(0xb106);
            
            // expected isn't the entire string but just the first portion long enough to encounter an abbreviation
            actual = actual.Substring(0, expected.Length);

            Assert.AreEqual(expected, actual);
        }
    }
}
