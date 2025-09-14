using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltFactory : IBoltFactory
    {
        private readonly IBoltViewRenter m_viewRenter;
        private readonly IBoltPresenterFactory m_presenterFactory;

        public BoltFactory(IBoltViewRenter viewRenter, IBoltPresenterFactory presenterFactory)
        {
            m_viewRenter = viewRenter;
            m_presenterFactory = presenterFactory;
        }

        public IBoltHandle Create(Vector3 position, Vector3 directionNormalized, ProjectileConfig config)
        {
            // Rent view and place at spawn
            var view = m_viewRenter.Rent(position);

            // Build model + presenter and wire them
            var presenter = m_presenterFactory.Create(view, config);

            // Compose the handle
            var handle = new BoltHandle(m_viewRenter, view, presenter);
            handle.Spawn(position, directionNormalized);
            return handle;
        }
    }
}
