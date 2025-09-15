namespace Scripts
{
    /// <summary>
    /// Enemy is visible and active. Movement stays enabled; visibility events can push us to OutOfScreen.
    /// </summary>
    public sealed class EnemyState_Active : EnemyStateBase
    {
        public static readonly EnemyState_Active Instance = new EnemyState_Active();
        private EnemyState_Active() { }

        public override string Name => "Active";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(true);
        }
    }
}
