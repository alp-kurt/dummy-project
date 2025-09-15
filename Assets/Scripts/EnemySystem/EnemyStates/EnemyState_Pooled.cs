namespace Scripts
{
    public sealed class EnemyState_Pooled : EnemyStateBase
    {
        public static readonly EnemyState_Pooled Instance = new EnemyState_Pooled();
        private EnemyState_Pooled() { }

        public override string Name => "Pooled";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(false);
            ctx.Pool();
        }
    }
}
