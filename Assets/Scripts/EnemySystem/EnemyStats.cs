using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Base data for any enemy type. Derive concrete assets (e.g., MinionStats, ChampionStats).
    /// </summary>
    public abstract class EnemyStats : ScriptableObject
    {
        [Header("Gameplay")]
        [Tooltip("Max health points for this enemy.")]
        [SerializeField, Min(1)] private int _maxHealth = 3;

        [Tooltip("Movement speed in world units per second.")]
        [SerializeField, Min(0f)] private float _movementSpeed = 2.5f;

        [Tooltip("Damage dealt to the player on contact.")]
        [SerializeField, Min(1)] private int _damage = 1;

        [Header("Visuals")]
        [Tooltip("Sprite used by the enemy renderer.")]
        [SerializeField] private Sprite _sprite;

        [Tooltip("Scale multiplier applied to the sprite at spawn (>= 0.1).")]
        [SerializeField, Min(0.1f)] private float _spriteScale = 1f;

        public int MaxHealth => _maxHealth;
        public float MovementSpeed => _movementSpeed;
        public int Damage => _damage;
        public Sprite Sprite => _sprite;
        public float SpriteScale => _spriteScale;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxHealth < 1) _maxHealth = 1;
            if (_movementSpeed < 0f) _movementSpeed = 0f;
            if (_damage < 1) _damage = 1;
            if (_spriteScale < 0.1f) _spriteScale = 0.1f;

            if (_sprite == null)
                Debug.LogWarning($"[EnemyStats] '{name}' has no Sprite assigned.", this);
        }
#endif
    }
}
