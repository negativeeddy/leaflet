namespace NegativeEddy.Leaflet.Memory;

public interface IRandomNumberGenerator
{
    void Seed(int seed);
    int GetNext(int range);
}
