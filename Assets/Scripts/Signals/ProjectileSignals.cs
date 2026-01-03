using UnityEngine;

namespace Scripts
{
    public struct ProjectileHitSignal
    {
        public ProjectileView View;
        public IDamageable Target;
    }

    public struct ProjectileDespawnedSignal
    {
        public ProjectileView View;
    }

    public struct BoltSpawnedSignal
    {
        public BoltView View;
        public Vector3 SpawnPosition;
        public Vector3 DirectionNormalized;
    }

    public struct BoltReturnedToPoolSignal
    {
        public BoltView View;
    }
}
