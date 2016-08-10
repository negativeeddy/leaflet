using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NegativeEddy.Leaflet.Instructions.Tests
{
    [TestClass()]
    public class ZOpcodeDefinitionTests
    {
        [TestMethod]
        public void OpcodeDefinitionConstructorTests()
        {
            var data = new Tuple<string, OperandCountType, ushort>[] {
                new Tuple<string, OperandCountType, ushort>("OP2_2", OperandCountType.OP2, 2),
                new Tuple<string, OperandCountType, ushort>("OP2_10", OperandCountType.OP2, 10),
                new Tuple<string, OperandCountType, ushort>("OP1_132", OperandCountType.OP1, 4),
                new Tuple<string, OperandCountType, ushort>("OP0_177", OperandCountType.OP0, 1),
                new Tuple<string, OperandCountType, ushort>("EXT_1", OperandCountType.EXT, 1),
                new Tuple<string, OperandCountType, ushort>("EXT_21", OperandCountType.EXT, 21),
                new Tuple<string, OperandCountType, ushort>("VAR_228", OperandCountType.VAR, 4),
            };

            foreach (var item in data)
            {
                // validate values are set correctly
                var first = new OpcodeIdentifier(item.Item1);
                Assert.AreEqual(item.Item2, first.OperandCount);
                Assert.AreEqual(item.Item3, first.OpcodeNumber);

                // validate equivalence
                var second = new OpcodeIdentifier(item.Item2, item.Item3);
                Assert.AreEqual(second, first);
            }
        }
    }
}