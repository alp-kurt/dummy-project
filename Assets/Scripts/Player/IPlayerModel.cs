using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public interface IPlayerModel
    {
        // Latest input vector provided by the presenter (normalized or not)
        IReadOnlyReactiveProperty<Vector2> MoveInput { get; }

        // High-level state the rest of the game can observe
        IReadOnlyReactiveProperty<PlayerMovementState> MovementState { get; }

        // Convenience boolean
        IReadOnlyReactiveProperty<bool> IsMoving { get; }

        // Edge events (fire only on transitions)
        IObservable<Unit> StartedMoving { get; }
        IObservable<Unit> StoppedMoving { get; }

        // Presenter calls this every frame (or when input changes)
        void SetMoveInput(Vector2 input);
    }
}
