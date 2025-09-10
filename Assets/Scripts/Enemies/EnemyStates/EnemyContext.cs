namespace Scripts
{
    /// <summary>
    /// Shared data for the FSM (owned by EnemyModel, passed to states).
    /// </summary>
    public sealed class EnemyContext
    {
        public EnemyModel Model { get; }
        public float OffscreenDespawnSeconds { get; }
        public float DeathDespawnSeconds { get; }

        public EnemyContext(EnemyModel model, float offscreenSec, float deathSec)
        {
            Model = model;
            OffscreenDespawnSeconds = offscreenSec;
            DeathDespawnSeconds = deathSec;
        }

        public bool IsOnScreen => Model.IsOnScreenInternal;
        public int Health => Model.Health.Value;
        public float MoveSpeed => Model.MoveSpeed;

        // Convenience pass-throughs for states
        public void SetCanMove(bool canMove) => Model.SetCanMoveInternal(canMove);
        public void EmitDied() => Model.EmitDiedInternal();
        public void Pool() => Model.SwitchToPooledInternal();
        public void Transition(IEnemyState next) => Model.StateMachineInternal.Transition(next);
    }
}
