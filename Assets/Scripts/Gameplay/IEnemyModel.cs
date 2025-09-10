using UniRx;

namespace Scripts
{
    public interface IEnemyModel
    {
        string Name { get; }
        EnemyArchetype Archetype { get; }

        IReadOnlyReactiveProperty<int> Health { get; }
        int MaxHealth { get; }
        int Damage { get; }
        float MoveSpeed { get; }

        IReadOnlyReactiveProperty<EnemyState> State { get; }

        void InitializeFromConfig(EnemyConfig config);
        void ApplyDamage(int amount);
        void SetState(EnemyState state);
        void ResetRuntimeState(); // for pooling reuse
    }
}
