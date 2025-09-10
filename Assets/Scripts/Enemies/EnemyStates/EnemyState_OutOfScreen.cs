using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts
{
    /// <summary>
    /// Starts a 2s timer to pool while off-screen.
    /// Cancels the timer when exiting (e.g., becomes visible).
    /// Movement remains enabled so enemy can walk into view.
    /// </summary>
    public sealed class EnemyState_OutOfScreen : EnemyStateBase
    {
        public override string Name => "OutOfScreen";

        private CancellationTokenSource m_cts;

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(true);
            m_cts = new CancellationTokenSource();
            DespawnCountdownAsync(ctx, m_cts.Token).Forget();
        }

        public override void OnExit(EnemyContext ctx)
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_cts = null;
        }

        public override void OnUpdate(EnemyContext ctx, float dt)
        {
            // No per-frame logic needed here; transitions are driven by visibility events in the model.
        }

        private async UniTaskVoid DespawnCountdownAsync(EnemyContext ctx, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(ctx.OffscreenDespawnSeconds), DelayType.DeltaTime, PlayerLoopTiming.Update, token);
                // Still off-screen? Pool.
                if (!ctx.IsOnScreen)
                {
                    ctx.Transition(new EnemyState_Pooled());
                }
            }
            catch (OperationCanceledException) { /* exit gracefully */ }
        }
    }
}
