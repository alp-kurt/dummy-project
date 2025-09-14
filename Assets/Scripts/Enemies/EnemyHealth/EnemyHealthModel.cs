using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyHealthModel : IEnemyHealthModel
    {
        private readonly ReactiveProperty<float> _current01 = new(1f);
        private readonly Subject<Unit> _died = new();
        private readonly Subject<int> _damaged = new();
        private readonly Subject<int> _healed = new();

        private bool _isDead;

        public IReadOnlyReactiveProperty<float> CurrentHealth01 => _current01;
        public int MaxHealth { get; private set; } = 1;

        public IObservable<Unit> Died => _died;
        public IObservable<int> Damaged => _damaged;
        public IObservable<int> Healed => _healed;

        public void Initialize(int maxHealth)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            ResetFull();
        }

        public void ResetFull()
        {
            _current01.Value = 1f;
            _isDead = false;
        }

        public void ReceiveDamage(int amountHp)
        {
            int dmg = Math.Max(0, amountHp);
            if (dmg <= 0 || _isDead) return;

            float new01 = Mathf.Max(0f, _current01.Value - (dmg / (float)MaxHealth));
            _current01.Value = new01;
            _damaged.OnNext(dmg);

            Debug.Log("Received Damage");

            if (!_isDead && new01 <= 0f)
            {
                _isDead = true;
                _died.OnNext(Unit.Default);
            }
        }
    }
}
