using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Core.IO
{
    public class InterpreterOutput : IZOutput
    {
        public InterpreterOutput()
        {
        }

        private Subject<string> _outputSubject = new Subject<string>();
        public IObservable<string> Print { get { return _outputSubject.AsObservable(); } }

        public void WriteOutputLine(string text = null)
        {
            if (text != null)
            {
                WriteOutput(text);
            }
            WriteOutput(Environment.NewLine);
        }

        public void WriteOutput(string text)
        {
            _outputSubject.OnNext(text);
        }

        public void WriteOutputLine(object text)
        {
            if (text != null)
            {
                WriteOutput(text);
            }
            WriteOutput(Environment.NewLine);
        }

        public void WriteOutput(object text)
        {
            _outputSubject.OnNext(text.ToString());
        }
    }
}
