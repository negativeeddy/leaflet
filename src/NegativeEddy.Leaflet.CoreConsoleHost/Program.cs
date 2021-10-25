using NegativeEddy.Leaflet;
using NegativeEddy.Leaflet.CoreConsoleHost;
using System.Diagnostics;

var zm = new Interpreter
{
    Input = new ConsoleInput(),
    DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Verbose,
};

zm.Output.Subscribe(x => Console.Write(x));
zm.Diagnostics.Subscribe(x => Debug.Write(x));

string filename = Path.Combine("GameFiles", "minizork.z3");
using (var stream = File.OpenRead(filename))
{
    zm.LoadStory(stream);
}
Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

//DumpObjectTree(zm);

//DumpObjects(zm);

while (zm.IsRunning)
{
    zm.ExecuteCurrentInstruction();
}
