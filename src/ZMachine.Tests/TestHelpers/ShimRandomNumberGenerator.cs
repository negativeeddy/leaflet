using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Core.Memory;

namespace ZMachine.Tests.TestHelpers
{
    class ShimRandomNumberGenerator : IRandomNumberGenerator
    {
        public int NextRandomNumberToGenerate { get; set; }

        public int GetNext(int range)
        {
            int next = NextRandomNumberToGenerate;
            Debug.Assert(next >= 1);
            Debug.Assert(next <= range);
            return next;
        }

        public void Seed(int seed)
        {
            Trace.WriteLine($"SEEDING RAND WITH {seed}");
        }
    }
}
