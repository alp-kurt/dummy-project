using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BoltConfig", menuName = "Configs/Projectiles/Bolt")]
    public sealed class BoltConfig : ProjectileConfig
    {
        [SerializeField, Min(0f)] private float _lifetimeSeconds = 6f; // 0 = infinite (opt-in)
        public float LifetimeSeconds => _lifetimeSeconds;
    }
}