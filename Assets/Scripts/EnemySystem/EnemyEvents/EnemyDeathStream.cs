using System;
using UniRx;

namespace Scripts
{
    public sealed class EnemyDeathStream : IEnemyDeathStream, IDisposable
    {
        private readonly Subject<Unit> _died = new Subject<Unit>();
        public IObservable<Unit> Died => _died;

        public void Publish() => _died.OnNext(Unit.Default);

        public void Dispose() => _died.Dispose();
    }
}