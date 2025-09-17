using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public interface IPlayerModel
    {
        // Movement
        Vector2 MoveInput { get; }
        bool IsWalking { get; }
        void SetMoveInput(Vector2 input);
        Vector3 Step(float deltaTime);

        // Health
        float MaxHealth { get; }
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }
        IObservable<Unit> Died { get; }
        void TakeDamage(float amount);
        void Die();
    }
}
