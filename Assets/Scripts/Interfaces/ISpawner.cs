using System;

namespace Scripts
{
    /// Generic spawner contract for “things that create T”.
    /// - Spawned: stream of each created instance
    /// - SpawnOne(): create exactly one immediately (returns the created instance)
    /// - Start()/Stop(): control the internal spawn loop, if any
    public interface ISpawner<out T>
    {
        IObservable<T> Spawned { get; }
        T SpawnOne();
        void Start();
        void Stop();
    }
}
