using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerModel : IPlayerModel, IDisposable
    {
        private const float Deadzone = 0.02f;

        // Movement
        public Vector2 MoveInput { get; private set; }
        public bool IsWalking { get; private set; }
        private readonly float _moveSpeed;

        // Health
        public float MaxHealth { get; }
        private readonly ReactiveProperty<float> _currentHealth;
        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
        private readonly Subject<Unit> _died = new();
        public IObservable<Unit> Died => _died;
        private bool _isDead;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public PlayerModel(float moveSpeed, float maxHealth)
        {
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            MaxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = new ReactiveProperty<float>(MaxHealth);
        }

        // Movement
        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
            IsWalking = input.sqrMagnitude >= Deadzone * Deadzone && !_isDead;
        }

        public Vector3 Step(float dt)
        {
            if (!IsWalking) return Vector3.zero;
            var v = MoveInput.normalized * _moveSpeed * dt;
            return new Vector3(v.x, v.y, 0f);
        }

        // Health
        public void TakeDamage(float amount)
        {
            if (_isDead) return;
            var dmg = Mathf.Max(0f, amount);
            if (dmg <= 0f) return;

            var next = Mathf.Max(0f, _currentHealth.Value - dmg);
            _currentHealth.Value = next;

            if (next <= 0f) Die();
        }

        public void Die()
        {
            if (_isDead) return;
            _isDead = true;
            _currentHealth.Value = 0f;
            _died.OnNext(Unit.Default);
            _signalBus.Fire(new PlayerDiedSignal { Model = this });
        }

        public void Dispose()
        {
            _died?.OnCompleted();
            _died?.Dispose();
            _currentHealth?.Dispose();
        }
    }
}
