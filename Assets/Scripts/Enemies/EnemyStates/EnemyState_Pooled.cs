namespace Scripts
{
    public sealed class EnemyState_Pooled : EnemyStateBase
    {
        public override string Name => "Pooled";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(false);
            ctx.PoolWithReason(ctx.PooledReason);
            ctx.PooledReason = EnemyPooledReason.Unknown; // reset
        }
    }
}
