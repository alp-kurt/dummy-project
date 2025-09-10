using UnityEngine;

namespace Scripts
{
    public interface IEnemyState
    {
        string Name { get; }
        void OnEnter(EnemyContext ctx);
        void OnUpdate(EnemyContext ctx, float deltaTime);
        void OnExit(EnemyContext ctx);
    }
}
