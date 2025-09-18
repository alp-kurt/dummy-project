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

        public IBoltHandle Create(Vector3 position, Vector3 directionNormalized, BoltConfig config)
        {
            var view = _viewRenter.Rent(position);

            // Optional scale override from BoltConfig
            if (config != null && config.ScaleOverride > 0f)
            {
                float s = config.ScaleOverride;
                view.CachedTransform.localScale = new Vector3(s, s, s);
            }

            var presenter = _presenterFactory.Create(view, config);

            var handle = new BoltHandle(_viewRenter, view, presenter);
            handle.Spawn(position, directionNormalized);
            return handle;
        }
    }
}