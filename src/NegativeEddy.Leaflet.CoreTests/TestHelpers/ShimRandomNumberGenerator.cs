using System.Diagnostics;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.TestHelpers
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
