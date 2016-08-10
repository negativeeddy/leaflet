using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegativeEddy.Leaflet.Core.Memory
{
    public interface IRandomNumberGenerator
    {
        void Seed(int seed);
        int GetNext(int range);
    }
}
    