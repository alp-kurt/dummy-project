using UnityEngine;

namespace Scripts
{
    public sealed class PlayerModel : IPlayerModel
    {
        private const float kDeadzone = 0.02f;

        public Vector2 MoveInput { get; private set; }
        public bool IsWalking { get; private set; }

        private readonly float m_speed;

        public PlayerModel(float speed = 2f)
        {
            m_speed = Mathf.Max(0f, speed);
        }

        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
            IsWalking = input.sqrMagnitude >= kDeadzone * kDeadzone;
        }

        public Vector3 Step(float dt)
        {
            if (!IsWalking) return Vector3.zero;
            var v = MoveInput.normalized * m_speed * dt;
            return new Vector3(v.x, v.y, 0f);
        }
    }
}
