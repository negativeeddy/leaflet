using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Core.IO
{
    public interface IZOutput
    {
        IObservable<string> Print { get; }
    }
}
