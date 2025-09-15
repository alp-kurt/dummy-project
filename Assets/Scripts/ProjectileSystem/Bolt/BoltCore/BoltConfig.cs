using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BoltConfig", menuName = "Game/Projectiles/Bolt")]
    public sealed class BoltConfig : ProjectileConfigBase
    {
        [Header("Lifetime")]
        [Tooltip("Lifetime in seconds. 0 = infinite (never expires).")]
        [SerializeField, Min(0f)] private float _lifetimeSeconds = 6f;

        public float LifetimeSeconds => _lifetimeSeconds;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lifetimeSeconds < 0f) _lifetimeSeconds = 0f;
        }
#endif
    }
}
