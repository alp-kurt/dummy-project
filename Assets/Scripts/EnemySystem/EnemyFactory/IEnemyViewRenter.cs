using UnityEngine;

namespace Scripts
{
    public interface IEnemyViewRenter
    {
        EnemyView Rent(Vector3 worldPosition);
        void Return(EnemyView view);
    }
}
