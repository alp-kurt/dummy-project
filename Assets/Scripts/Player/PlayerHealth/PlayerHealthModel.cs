using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class PlayerHealthModel : IPlayerHealthModel, IDamageable
    {
        private const float k_DefaultMaxHealth = 20f;

        private readonly ReactiveProperty<float> _current01 = new ReactiveProperty<float>(1f);
        private readonly Subject<Unit> _died = new Subject<Unit>();
        private readonly Subject<int> _damaged = new Subject<int>();
        private readonly Subject<int> _healed = new Subject<int>();

        private bool _isDead;

        public float MaxHealth { get; private set; }
        public IReadOnlyReactiveProperty<float> CurrentHealth01 => _current01;
        public IObservable<Unit> Died => _died;
        public IObservable<int> Damaged => _damaged;
        public IObservable<int> Healed => _healed;

        public PlayerHealthModel(float maxHealth = k_DefaultMaxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            _current01.Value = 1f;
            _isDead = false;
        }

        public void ReceiveDamage(int amountHp)
        {
            int dmg = Math.Max(0, amountHp);
            if (dmg <= 0) return;

            // If already dead, ignore further damage (prevents re-emits)
            if (_isDead) return;

            float new01 = Mathf.Max(0f, _current01.Value - (dmg / MaxHealth));
            _current01.Value = new01;
            _damaged.OnNext(dmg);

            if (new01 <= 0f && !_isDead)
            {
                _isDead = true;
                _died.OnNext(Unit.Default); // emit once
            }
        }

        public void Heal(int amountHp)
        {
            int heal = Math.Max(0, amountHp);
            if (heal <= 0) return;

            float new01 = Mathf.Min(1f, _current01.Value + (heal / MaxHealth));
            _current01.Value = new01;
            _healed.OnNext(heal);

            // Note: there is no auto-“resurrect” here. Use ResetFull() to revive+refill.
        }

        // Note: a usefull function to enable reviving functionality upon a rewarded ad signal
        public void ResetFull()
        {
            _current01.Value = 1f;
            _isDead = false;
        }
    }
}
