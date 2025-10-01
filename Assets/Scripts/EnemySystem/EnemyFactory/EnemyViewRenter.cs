using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Pool façade: rent/return EnemyView, nothing else.
    /// </summary>
    public sealed class EnemyViewRenter : IEnemyViewRenter
    {
        private readonly IObjectPool<EnemyView> _pool;

        public EnemyViewRenter(IObjectPool<EnemyView> pool)
        {
            _pool = pool;
        }

        public EnemyView Rent(Vector3 worldPosition)
        {
            var view = _pool.GetObject();

            // Baseline reset to avoid state bleeding
            view.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            view.transform.localScale = Vector3.one;

            return view;
        }

        public void Return(EnemyView view)
        {
            _pool.ReleaseObject(view);
        }
    }
}
