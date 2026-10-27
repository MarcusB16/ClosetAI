namespace ClosetAPI.Domain.Outfits;

public class SeededRandom(int seed)
{
    private int _state = seed;

    public double NextDouble()
    {
        unchecked
        {
            _state += 0x6d2b79f5;
            var t = (uint)_state;
            t = (t ^ (t >> 15)) * (1 | t);
            t = (t + (t ^ (t >> 7)) * (61 | t)) ^ t;
            return (t ^ (t >> 14)) / 4294967296.0;
        }
    }

    public int NextIndex(int count) => (int)Math.Floor(NextDouble() * count);
}
