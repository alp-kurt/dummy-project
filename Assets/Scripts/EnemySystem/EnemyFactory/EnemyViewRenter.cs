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

            // Baseline reset to avoid state bleeding
            view.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            view.transform.localScale = Vector3.one;

            return view;
        }

        public void Return(EnemyView view)
        {
            m_pool.ReleaseObject(view);
        }
    }
}
