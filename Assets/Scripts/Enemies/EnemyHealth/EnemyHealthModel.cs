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

        private bool _isDead;

        public IReadOnlyReactiveProperty<float> CurrentHealth01 => _current01;
        public int MaxHealth { get; private set; }

        public IObservable<Unit> Died => _died;
        public IObservable<int> Damaged => _damaged;

        public void Initialize(int maxHealth)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            _current01.Value = 1f;
            _isDead = false;
        }

        public void ResetFull()
        {
            _current01.Value = 1f;
            _isDead = false;
        }

        public void ReceiveDamage(int dmg)
        {
            if (_isDead || dmg <= 0) return;

            float new01 = Mathf.Max(0f, _current01.Value - (dmg / (float)MaxHealth));
            _current01.Value = new01;
            _damaged.OnNext(dmg);

            if (!_isDead && new01 <= 0f)
            {
                _isDead = true;
                _died.OnNext(Unit.Default);
            }
        }
    }
}
