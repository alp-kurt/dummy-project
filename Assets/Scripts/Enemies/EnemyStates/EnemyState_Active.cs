namespace Scripts
{
    /// <summary>
    /// Enemy is visible and active. Nothing special here.
    /// Movement stays enabled; visibility events can push us to OutOfScreen.
    /// </summary>
    public sealed class EnemyState_Active : EnemyStateBase
    {
        public override string Name => "Active";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(true);
        }
    }
}
