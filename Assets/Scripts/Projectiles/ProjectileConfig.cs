using UnityEngine;

namespace Scripts
{
    public abstract class ProjectileConfig : ScriptableObject
    {
        [SerializeField] private string _displayName = "Projectile";
        [SerializeField] private Sprite _sprite;
        [SerializeField] private int _baseDamage = 1;
        [SerializeField] private float _baseSpeed = 6f;

        public string DisplayName => _displayName;
        public Sprite Sprite => _sprite;
        public int BaseDamage => _baseDamage;
        public float BaseSpeed => _baseSpeed;
    }
}
