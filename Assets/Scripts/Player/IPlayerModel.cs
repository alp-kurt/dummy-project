using UnityEngine;

namespace Scripts
{
    public interface IPlayerModel
    {
        Vector2 MoveInput { get; }
        bool IsWalking { get; }

        void SetMoveInput(Vector2 input);
        Vector3 Step(float deltaTime);
    }
}
