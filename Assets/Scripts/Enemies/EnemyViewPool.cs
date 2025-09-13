using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Typed pool that re-parents EnemyView under pooled/active roots automatically.
    /// </summary>
    public sealed class EnemyViewPool : AbstractObjectPool<EnemyView>
    {
        private readonly Transform m_pooledParent;
        private readonly Transform m_activeParent;

        // DI supplies factory; installer supplies min/max + parents
        public EnemyViewPool(
            IObjectFactory<EnemyView> factory,
            int min, int max,
            Transform pooledParent,
            Transform activeParent
        ) : base(factory, min, max)
        {
            m_pooledParent = pooledParent;
            m_activeParent = activeParent;
        }

        protected override void OnCreated(EnemyView instance)
        {
            if (m_pooledParent != null)
                instance.transform.SetParent(m_pooledParent, false);

            base.OnCreated(instance); // default: inactive
        }

        protected override void OnRented(EnemyView instance)
        {
            if (m_activeParent != null)
                instance.transform.SetParent(m_activeParent, false);

            base.OnRented(instance);  // default: OnRent() activates
        }

        protected override void OnReleased(EnemyView instance)
        {
            base.OnReleased(instance); // default: OnRelease() deactivates
            if (m_pooledParent != null)
                instance.transform.SetParent(m_pooledParent, false);
        }
    }
}
