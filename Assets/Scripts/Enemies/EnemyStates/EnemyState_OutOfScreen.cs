using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts
{
    /// <summary>
    /// Enemy left the screen. By default we keep them moving so they can re-enter.
    /// If they remain off-screen beyond OffscreenDespawnSeconds, they are pooled.
    /// </summary>
    public sealed class EnemyState_OutOfScreen : EnemyStateBase
    {
        public static readonly EnemyState_OutOfScreen Instance = new EnemyState_OutOfScreen();
        private EnemyState_OutOfScreen() { }

        public override string Name => "OutOfScreen";

        public override void OnEnter(EnemyContext ctx)
        {
            // Keep moving off-screen so they can naturally re-enter (tune to your policy)
            ctx.SetCanMove(true);

            // Per-enemy CTS lives on the context
            var cts = new CancellationTokenSource();
            ctx.StartOffscreenTimer(cts);

            DespawnCountdownAsync(ctx, cts.Token).Forget();
        }

        public override void OnExit(EnemyContext ctx)
        {
            ctx.CancelOffscreenTimer();
        }

        private async UniTaskVoid DespawnCountdownAsync(EnemyContext ctx, CancellationToken token)
        {
            try
            {
                var delayType = ctx.UseUnscaledTime ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;

                await UniTask.Delay(TimeSpan.FromSeconds(ctx.OffscreenDespawnSeconds),
                                    delayType, PlayerLoopTiming.Update, token);

                if (!token.IsCancellationRequested && !ctx.IsOnScreen)
                {
                    ctx.Transition(EnemyState_Pooled.Instance);
                }
            }
            catch (OperationCanceledException) { /* expected on exit */ }
        }
    }
}
