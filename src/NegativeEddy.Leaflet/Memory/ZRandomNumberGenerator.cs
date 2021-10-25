namespace NegativeEddy.Leaflet.Memory;

class ZRandomNumberGenerator : IRandomNumberGenerator
{
    private Random _rand = new ();

    public int GetNext(int range)
    {
        if (range <= 0)
        {
            // negative or zero range means reseed and return 0
            Seed(range);
            return 0;
        }
        else
        {
            // otherwise return random from 1 to range (inclusive)
            return _rand.Next(1, range + 1);
        }
    }

    public void Seed(int seed)
    {
        if (seed == 0)
        {
            // 0 seed means use a random seed
            _rand = new Random();
        }
        else
        {
            _rand = new Random(seed);
        }
    }
}
