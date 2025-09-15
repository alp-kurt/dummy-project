using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BoltConfig", menuName = "Game/Projectiles/Bolt Config")]
    public sealed class BoltConfig : ProjectileConfigBase
    {
        [Header("Lifetime")]
        [Tooltip("Lifetime in seconds. 0 = infinite (never expires).")]
        [SerializeField, Min(0f)] private float _lifetimeSeconds = 6f;
        public float LifetimeSeconds => _lifetimeSeconds;

        [Header("Visuals (Optional)")]
        [Tooltip("If > 0, overrides the spawned BoltView's localScale uniformly. Leave 0 to use the prefab's scale.")]
        [SerializeField, Min(0f)] private float _scaleOverride = 0f;
        public float ScaleOverride => _scaleOverride;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lifetimeSeconds < 0f) _lifetimeSeconds = 0f;
            if (_scaleOverride < 0f) _scaleOverride = 0f;
        }
#endif
    }
}
