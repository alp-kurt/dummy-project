using System.Threading;

namespace Scripts
{
    /// <summary>
    /// Thin facade the states use to interact with the enemy without knowing about the presenter/view.
    /// Holds per-enemy timer CTS so singleton states don't share state across enemies.
    /// </summary>
    public sealed class EnemyContext
    {
        public EnemyModel Model { get; }
        public float OffscreenDespawnSeconds { get; }
        public float DeathDespawnSeconds { get; }
        /// <summary>When true, state timers use unscaled time (ignores Time.timeScale).</summary>
        public bool UseUnscaledTime { get; }

        // Per-enemy cancellation tokens for state timers
        internal CancellationTokenSource OffscreenCts { get; private set; }
        internal CancellationTokenSource DeathCts { get; private set; }

        public EnemyContext(EnemyModel model, float offscreenSec, float deathSec)
            : this(model, offscreenSec, deathSec, false) { }

        public EnemyContext(EnemyModel model, float offscreenSec, float deathSec, bool useUnscaledTime)
        {
            Model = model;
            OffscreenDespawnSeconds = offscreenSec;
            DeathDespawnSeconds = deathSec;
            UseUnscaledTime = useUnscaledTime;
        }

        public bool IsOnScreen => Model.IsOnScreenInternal;
        public float MoveSpeed => Model.MoveSpeed;

        public void SetCanMove(bool canMove) => Model.SetCanMoveInternal(canMove);
        public void EmitDied() => Model.EmitDiedInternal();
        public void Pool() => Model.SwitchToPooledInternal();
        public void Transition(IEnemyState next) => Model.StateMachineInternal.Transition(next);

        // Timer helpers for singleton states
        public void StartOffscreenTimer(CancellationTokenSource cts)
        {
            CancelOffscreenTimer();
            OffscreenCts = cts;
        }

        public void CancelOffscreenTimer()
        {
            OffscreenCts?.Cancel();
            OffscreenCts?.Dispose();
            OffscreenCts = null;
        }

        public void StartDeathTimer(CancellationTokenSource cts)
        {
            CancelDeathTimer();
            DeathCts = cts;
        }

        public void CancelDeathTimer()
        {
            DeathCts?.Cancel();
            DeathCts?.Dispose();
            DeathCts = null;
        }

        public void CancelAllTimers()
        {
            CancelOffscreenTimer();
            CancelDeathTimer();
        }
    }
}
