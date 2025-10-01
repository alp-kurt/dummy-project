using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Pure data model: movement flags, reactive health, and signals. No FSM/context.
    /// </summary>
    public sealed class EnemyModel : IEnemyModel, IDamageable
    {
        // Stats / movement
        private float _moveSpeed;
        private readonly ReactiveProperty<bool> _canMove = new(false);
        private int _damage;

        // Health
        private int _maxHealth;
        private readonly ReactiveProperty<int> _currentHealth = new(1);
        private readonly Subject<int> _damaged = new();
        private readonly Subject<Unit> _died = new();
        private bool _isDead;

        public void SetCanMove(bool value) => _canMove.Value = value;

        // Public
        public float MoveSpeed => _moveSpeed;
        public int Damage => _damage;
        public IReadOnlyReactiveProperty<bool> CanMove => _canMove;

        public int MaxHealth => _maxHealth;
        public IReadOnlyReactiveProperty<int> CurrentHealth => _currentHealth;
        public IObservable<int> Damaged => _damaged;
        public IObservable<Unit> Died => _died;

        public void Initialize(EnemyStats stats)
        {
            _moveSpeed = Mathf.Max(0f, stats.MovementSpeed);
            _damage = Mathf.Max(0, stats.Damage);
            _maxHealth = Mathf.Max(1, stats.MaxHealth);

            _currentHealth.Value = _maxHealth;
            _isDead = false;
            _canMove.Value = false;
        }

        public void ResetForSpawn()
        {
            _isDead = false;
            _currentHealth.Value = _maxHealth;
            _canMove.Value = true;
        }

        // Damage intake
        public void ReceiveDamage(int amount)
        {
            if (_isDead) return;
            var dmg = Mathf.Max(0, amount);
            if (dmg == 0) return;

            var next = Mathf.Max(0, _currentHealth.Value - dmg);
            _currentHealth.Value = next;
            _damaged.OnNext(dmg);

            if (next <= 0) Die();
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;
            _currentHealth.Value = 0;
            _canMove.Value = false;
            _died.OnNext(Unit.Default);
        }
    }
}
