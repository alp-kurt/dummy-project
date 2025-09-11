using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts
{
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

        private async UniTaskVoid DespawnCountdownAsync(EnemyContext ctx, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(ctx.OffscreenDespawnSeconds),
                                    DelayType.DeltaTime, PlayerLoopTiming.Update, token);
                if (!ctx.IsOnScreen)
                {
                    ctx.Transition(new EnemyState_Pooled());
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
