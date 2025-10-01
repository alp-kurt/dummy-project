using UnityEngine;

namespace Scripts
{
    public abstract class ProjectileConfigBase : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("In-game display name for this projectile.")]
        [SerializeField] private string _displayName = "Projectile";

        [Tooltip("Sprite used by the projectile renderer.")]
        [SerializeField] private Sprite _sprite;

        [Header("Base Stats")]
        [Tooltip("Base damage per hit before runtime modifiers.")]
        [SerializeField, Min(1)] private int _baseDamage = 1;

        [Tooltip("Base speed in world units per second.")]
        [SerializeField, Min(0f)] private float _baseSpeed = 6f;

        public string DisplayName => _displayName;
        public Sprite Sprite => _sprite;
        public int BaseDamage => _baseDamage;
        public float BaseSpeed => _baseSpeed;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _displayName = string.IsNullOrWhiteSpace(_displayName) ? "Projectile" : _displayName.Trim();

            if (_baseDamage < 1) _baseDamage = 1;
            if (_baseSpeed < 0f) _baseSpeed = 0f;

            if (_sprite == null)
                Debug.LogWarning($"[ProjectileConfig] '{name}' has no Sprite assigned.", this);

            if (_baseSpeed == 0f)
                Debug.LogWarning($"[ProjectileConfig] '{name}' base speed is 0 (won't move).", this);
        }
#endif
    }
}
