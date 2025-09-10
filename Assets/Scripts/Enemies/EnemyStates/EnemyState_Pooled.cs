namespace Scripts
{
    /// <summary>
    /// Terminal state: notify pool.
    /// </summary>
    public sealed class EnemyState_Pooled : EnemyStateBase
    {
        public override string Name => "Pooled";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(false);
            ctx.Pool(); // Emits ReturnedToPool for the pool to reclaim
        }
    }
}
