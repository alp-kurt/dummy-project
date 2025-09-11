using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts
{
    public sealed class EnemyState_Dead : EnemyStateBase
    {
        public override string Name => "Dead";
        private CancellationTokenSource m_cts;

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(false);
            ctx.EmitDied();
            m_cts = new CancellationTokenSource();
            DespawnAfterAsync(ctx, m_cts.Token).Forget();
        }

        public override void OnExit(EnemyContext ctx)
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_cts = null;
        }

        private async UniTaskVoid DespawnAfterAsync(EnemyContext ctx, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(ctx.DeathDespawnSeconds),
                                    DelayType.DeltaTime, PlayerLoopTiming.Update, token);
                ctx.Transition(new EnemyState_Pooled());
            }
            catch (OperationCanceledException) { }
        }
    }
}
