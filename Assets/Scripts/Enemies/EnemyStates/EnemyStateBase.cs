namespace Scripts
{
    public abstract class EnemyStateBase : IEnemyState
    {
        public abstract string Name { get; }

        public virtual void OnEnter(EnemyContext ctx) { }
        public virtual void OnUpdate(EnemyContext ctx, float deltaTime) { }
        public virtual void OnExit(EnemyContext ctx) { }
    }
}
