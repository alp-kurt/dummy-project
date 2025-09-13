namespace Scripts
{
    public interface IPoolPolicy
    {
        int Min { get; }
        int Max { get; }
    }

    public struct PoolPolicy : IPoolPolicy
    {
        public int Min { get; }
        public int Max { get; }

        public PoolPolicy(int min, int max) { Min = min; Max = max; }
    }
}
