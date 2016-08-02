using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Tests.Infrastructure
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void ConcatToStringTest()
        {
            int[] values = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            string expected = "0,1,2,3,4,5,6,7,8,9,10";
            string actual = values.ConcatToString();
            Assert.AreEqual(expected, actual);


            expected = "0:1:2:3:4:5:6:7:8:9:10";
            actual = values.ConcatToString(':');
            Assert.AreEqual(expected, actual);

            expected = "0x0000:0x0001:0x0002:0x0003:0x0004:0x0005:0x0006:0x0007:0x0008:0x0009:0x000a";
            actual = values.ConcatToString(':', x => "0x" + x.ToString("x4"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void StringBuilderAppendTest()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('1');

            string expected = "1";
            string actual = sb.ToString();
            Assert.AreEqual(expected, actual);

            sb.Append('X', 5);
            expected = "1XXXXX";
            actual = sb.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}
