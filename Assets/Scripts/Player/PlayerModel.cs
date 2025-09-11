using System;
using UniRx;

namespace Scripts
{
    public sealed class PlayerModel : IPlayerModel
    {
        // Backing health in HP units; UI binds to normalized value
        private const float k_MaxHealth = 100f;

        private readonly ReactiveProperty<float> _currentHealth01 = new(1f);
        private readonly Subject<Unit> _died = new();

        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth01;
        public float MaxHealth => k_MaxHealth;
        public IObservable<Unit> Died => _died;

        public void ApplyDamage(int amount)
        {
            int dmg = Math.Max(0, amount);
            if (dmg <= 0) return;

            // Convert incoming integer damage to normalized [0..1]
            float delta01 = dmg / k_MaxHealth;

            float new01 = Math.Max(0f, _currentHealth01.Value - delta01);
            _currentHealth01.Value = new01;

            if (new01 <= 0f)
            {
                _died.OnNext(Unit.Default);
            }
        }
    }
}
