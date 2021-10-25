using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace NegativeEddy.Leaflet.IO;

public class InterpreterOutput : IZOutput
{
    public InterpreterOutput()
    {
    }

    private readonly Subject<string> _outputSubject = new();
    public IObservable<string> Print { get { return _outputSubject.AsObservable(); } }

    public void WriteOutputLine(string? text = null)
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
        _outputSubject.OnNext(text.ToString() ?? String.Empty);
    }
}
