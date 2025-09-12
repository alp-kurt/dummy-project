using UniRx;

namespace Scripts
{
    public sealed class EnemyKillCounterModel : IEnemyKillCounterModel
    {
        private readonly ReactiveProperty<int> _count = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> Count => _count;
        public void Increment() => _count.Value = _count.Value + 1;
        public void Reset() => _count.Value = 0;
    }
}