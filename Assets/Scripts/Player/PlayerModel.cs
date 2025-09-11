using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class PlayerModel : IPlayerModel
    {
        // Deadzone avoids jitter when joystick rests near zero
        private const float k_MoveDeadzone = 0.02f;

        private readonly ReactiveProperty<Vector2> m_moveInput = new ReactiveProperty<Vector2>(Vector2.zero);
        private readonly ReactiveProperty<PlayerMovementState> m_movementState =
            new ReactiveProperty<PlayerMovementState>(PlayerMovementState.Idle);
        private readonly ReadOnlyReactiveProperty<bool> m_isMoving;

        private readonly Subject<Unit> m_startedMoving = new Subject<Unit>();
        private readonly Subject<Unit> m_stoppedMoving = new Subject<Unit>();

        public IReadOnlyReactiveProperty<Vector2> MoveInput => m_moveInput;
        public IReadOnlyReactiveProperty<PlayerMovementState> MovementState => m_movementState;
        public IReadOnlyReactiveProperty<bool> IsMoving => m_isMoving;

        public IObservable<Unit> StartedMoving => m_startedMoving;
        public IObservable<Unit> StoppedMoving => m_stoppedMoving;

        public PlayerModel()
        {
            // Derive IsMoving from MovementState
            m_isMoving = m_movementState
                .Select(s => s == PlayerMovementState.Moving)
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty();
        }

        public void SetMoveInput(Vector2 input)
        {
            // Store raw input for the presenter/view to consume
            m_moveInput.Value = input;

            // Compute state
            bool moving = input.sqrMagnitude >= k_MoveDeadzone * k_MoveDeadzone;
            var nextState = moving ? PlayerMovementState.Moving : PlayerMovementState.Idle;

            if (nextState != m_movementState.Value)
            {
                var prev = m_movementState.Value;
                m_movementState.Value = nextState;

                if (nextState == PlayerMovementState.Moving)
                    m_startedMoving.OnNext(Unit.Default);
                else
                    m_stoppedMoving.OnNext(Unit.Default);
            }
        }
    }
}
