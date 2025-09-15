using UnityEngine;

namespace Scripts
{
    public sealed class BoltFactory : IBoltFactory
    {
        private readonly IBoltViewRenter _viewRenter;
        private readonly IBoltPresenterFactory _presenterFactory;

        public BoltFactory(IBoltViewRenter viewRenter, IBoltPresenterFactory presenterFactory)
        {
            _viewRenter = viewRenter;
            _presenterFactory = presenterFactory;
        }

        public IBoltHandle Create(Vector3 position, Vector3 directionNormalized, ProjectileConfigBase config)
        {
            // Rent view and place at spawn
            var view = _viewRenter.Rent(position);

            // Build model + presenter and wire them
            var presenter = _presenterFactory.Create(view, config);

            // Compose the handle
            var handle = new BoltHandle(_viewRenter, view, presenter);
            handle.Spawn(position, directionNormalized);
            return handle;
        }
    }
}
