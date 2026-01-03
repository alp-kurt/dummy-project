using UnityEngine;

namespace Scripts
{
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
