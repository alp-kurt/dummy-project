using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltFactory : IBoltFactory
    {
        private readonly BoltViewPool _viewPool;
        private readonly IBoltPresenterFactory _presenterFactory;
        private readonly SignalBus _signalBus;

        public BoltFactory(
            BoltViewPool viewPool,
            IBoltPresenterFactory presenterFactory,
            SignalBus signalBus
        )
        {
            _viewPool = viewPool;
            _presenterFactory = presenterFactory;
            _signalBus = signalBus;
        }

        public IBoltHandle Create(Vector3 position, Vector3 directionNormalized, BoltConfig config)
        {
            var view = _viewPool.Spawn();
            view.CachedTransform.SetPositionAndRotation(position, Quaternion.identity);
            view.CachedTransform.localScale = Vector3.one;

            // Optional scale override from BoltConfig
            if (config != null && config.ScaleOverride > 0f)
            {
                float s = config.ScaleOverride;
                view.CachedTransform.localScale = new Vector3(s, s, s);
            }

            var presenter = _presenterFactory.Create(view, config);

            var handle = new BoltHandle(_viewPool, view, presenter, _signalBus);
            handle.Spawn(position, directionNormalized);

            _signalBus.Fire(new BoltSpawnedSignal
            {
                View = view,
                SpawnPosition = position,
                DirectionNormalized = directionNormalized
            });

            return handle;
        }
    }
}
