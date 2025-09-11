using System;
using UniRx;

namespace Scripts
{
    public sealed class PlayerHealthModel : IPlayerHealthModel
    {
        private const float k_DefaultMaxHealth = 20f;

        private readonly ReactiveProperty<float> _current01 = new(1f);
        private readonly Subject<Unit> _died = new();

        public float MaxHealth { get; private set; } = k_DefaultMaxHealth;
        public IReadOnlyReactiveProperty<float> CurrentHealth01 => _current01;
        public IObservable<Unit> Died => _died;

        public void ApplyDamage(int amountHp)
        {
            int dmg = Math.Max(0, amountHp);
            if (dmg <= 0) return;

            float new01 = Math.Max(0f, _current01.Value - (dmg / MaxHealth));
            _current01.Value = new01;

            if (new01 <= 0f)
            {
                _died.OnNext(Unit.Default);
            }
        }

        public void Heal(int amountHp)
        {
            int heal = Math.Max(0, amountHp);
            if (heal <= 0) return;

            float new01 = Math.Min(1f, _current01.Value + (heal / MaxHealth));
            _current01.Value = new01;
        }

        public void ResetFull()
        {
            _current01.Value = 1f;
        }
    }
}
