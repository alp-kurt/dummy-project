using System;   
using UniRx;

namespace Scripts
{
    public interface IBoltHandle
    {
        BoltView View { get; }
        BoltPresenter Presenter { get; }
        IObservable<Unit> ReturnedToPool { get; }

        void Spawn(UnityEngine.Vector3 position, UnityEngine.Vector3 directionNormalized);
        void Despawn();
        void Release();
    }
}
