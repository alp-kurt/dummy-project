using System;

namespace Scripts
{
    public interface IObjectPool<T>
    {
        T GetObject();
        void ReleaseObject(T obj);

        int AvailableCount { get; }
        int InUseCount { get; }

        // Optional observables (UniRx-friendly but plain .NET here)
        IObservable<T> Rented { get; }
        IObservable<T> Released { get; }
    }
}
