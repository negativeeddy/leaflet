using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegativeEddy.Leaflet.IO
{
    public interface IZOutput
    {
        IObservable<string> Print { get; }
    }
}
