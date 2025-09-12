using UniRx;

namespace Scripts
{
    public interface IEnemyKillCounterModel
    {
        IReadOnlyReactiveProperty<int> Count { get; }
        void Increment();
        void Reset();
    }
}