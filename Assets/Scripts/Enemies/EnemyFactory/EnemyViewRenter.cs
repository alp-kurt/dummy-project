using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Pool façade: rent/return EnemyView, nothing else.
    /// </summary>
    public sealed class EnemyViewRenter : IEnemyViewRenter
    {
        private readonly IObjectPool<EnemyView> m_pool;

        public EnemyViewRenter(IObjectPool<EnemyView> pool)
        {
            m_pool = pool;
        }

        public EnemyView Rent(Vector3 worldPosition)
        {
            var view = m_pool.GetObject();
            view.transform.position = worldPosition;
            return view;
        }

        public void Return(EnemyView view)
        {
            m_pool.ReleaseObject(view);
        }
    }
}
