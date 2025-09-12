using System;
using UnityEngine;

namespace Scripts
{
    public interface IPlayerModel
    {
        Vector2 MoveInput { get; }
        PlayerMovementState MovementState { get; }

        void SetMoveInput(Vector2 input);
        Vector3 Step(float deltaTime); // ← Model owns speed & motion math

        event Action<PlayerMovementState> OnMovementStateChanged;
    }
}
