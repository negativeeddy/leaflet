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
            var data = new (string opcodeName, OperandCountType countType, ushort opcodeNumber)[] {
                ("OP2_2", OperandCountType.OP2, 2),
                ("OP2_10", OperandCountType.OP2, 10),
                ("OP1_132", OperandCountType.OP1, 4),
                ("OP0_177", OperandCountType.OP0, 1),
                ("EXT_1", OperandCountType.EXT, 1),
                ("EXT_21", OperandCountType.EXT, 21),
                ("VAR_228", OperandCountType.VAR, 4),
            };

            foreach (var item in data)
            {
                // validate values are set correctly
                var first = new OpcodeIdentifier(item.opcodeName);
                Assert.AreEqual(item.countType, first.OperandCount);
                Assert.AreEqual(item.opcodeNumber, first.OpcodeNumber);

                // validate equivalence
                var second = new OpcodeIdentifier(item.countType, item.opcodeNumber);
                Assert.AreEqual(second, first);
            }
        }
    }
}