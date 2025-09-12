using System;
using UnityEngine;

namespace Scripts
{
    public sealed class PlayerModel : IPlayerModel
    {
        private const float kDeadzone = 0.02f;

        public Vector2 MoveInput { get; private set; }
        public PlayerMovementState MovementState { get; private set; } = PlayerMovementState.Idle;

        private readonly float m_speed;

        public event Action<PlayerMovementState> OnMovementStateChanged;

        public PlayerModel(float speed = 4f) { m_speed = Mathf.Max(0f, speed); }

        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
            var moving = input.sqrMagnitude >= kDeadzone * kDeadzone;
            var next = moving ? PlayerMovementState.Moving : PlayerMovementState.Idle;
            if (next != MovementState)
            {
                MovementState = next;
                OnMovementStateChanged?.Invoke(MovementState);
            }
        }

        public Vector3 Step(float dt)
        {
            if (MovementState == PlayerMovementState.Idle) return Vector3.zero;
            var v = MoveInput.normalized * m_speed * dt;
            return new Vector3(v.x, v.y, 0f);
        }
    }
}
