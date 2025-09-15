using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts
{
    public sealed class EnemyState_Dead : EnemyStateBase
    {
        public static readonly EnemyState_Dead Instance = new EnemyState_Dead();
        private EnemyState_Dead() { }

        public override string Name => "Dead";

        public override void OnEnter(EnemyContext ctx)
        {
            ctx.SetCanMove(false);
            ctx.EmitDied();

            var cts = new CancellationTokenSource();
            ctx.StartDeathTimer(cts);

            DespawnAfterAsync(ctx, cts.Token).Forget();
        }

        public override void OnExit(EnemyContext ctx)
        {
            ctx.CancelDeathTimer();
        }

        private async UniTaskVoid DespawnAfterAsync(EnemyContext ctx, CancellationToken token)
        {
            try
            {
                var delayType = ctx.UseUnscaledTime ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;

                await UniTask.Delay(TimeSpan.FromSeconds(ctx.DeathDespawnSeconds),
                                    delayType, PlayerLoopTiming.Update, token);

                if (!token.IsCancellationRequested)
                {
                    ctx.Transition(EnemyState_Pooled.Instance);
                }
            }
            catch (OperationCanceledException) { /* expected on exit */ }
        }
    }
}
