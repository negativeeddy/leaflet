namespace NegativeEddy.Leaflet.IO;

public interface IZOutput
{
    IObservable<string?> Print { get; }
}
