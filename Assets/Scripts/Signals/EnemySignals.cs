using UnityEngine;

namespace Scripts
{
    public struct EnemySpawnedSignal
    {
        public EnemyView View;
        public EnemyStats Stats;
        public Vector3 SpawnPosition;
    }

    public struct EnemyDiedSignal
    {
        public EnemyView View;
        public EnemyStats Stats;
    }

    public struct EnemyReturnedToPoolSignal
    {
        public EnemyView View;
    }
}
