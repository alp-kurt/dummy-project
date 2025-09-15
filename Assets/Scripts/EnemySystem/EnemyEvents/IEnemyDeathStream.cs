using System;
using UniRx;

namespace Scripts
{
    public interface IEnemyDeathStream
    {
        IObservable<Unit> Died { get; }
        void Publish(); 
    }
}