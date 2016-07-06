using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZMachine.Instructions;

namespace ZMachine.Memory.Tests
{
    [TestClass()]
    public class ZOpcodeTests
    {
        [TestMethod]
        public void VarOperandTypeTest()
        {
            byte b = Convert.ToByte("00101111", 2);

        }
        
        [TestMethod()]
        public void OpCodeTest()
        {
            Assert.Inconclusive();
            var inputs = new List<Tuple<byte[], OpcodeForm, ushort, OperandTypes[]>>()
            {
                new Tuple<byte[], OpcodeForm, ushort, OperandTypes[]>(new byte[] { 0x01, 0x02, 0x00 }, OpcodeForm.Short, 0x0102, new [] { OperandTypes.Omitted }),
            };

            foreach (var input in inputs)
            {
                byte[] data = input.Item1;
                ZOpcode zop = new ZOpcode(data, 0);

                OpcodeForm form = input.Item2;
                ushort opcode = input.Item3;
                OperandTypes[] operandTypes = input.Item4;

                Assert.AreEqual(form, zop.Form, "Form");
                Assert.AreEqual(opcode, zop.Opcode, "Opcde");
                Assert.AreEqual(operandTypes.Length, zop.OperandType.Count, "OperandTypes Count");
                for (int i = 0; i < zop.OperandType.Count; i++)
                {
                    Assert.AreEqual(operandTypes[i], zop.OperandType[i], $"OperandTypes {i}");
                }
            }
        }
    }
}