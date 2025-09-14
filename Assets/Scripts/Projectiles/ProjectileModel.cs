using UniRx;
using UnityEngine;

namespace Scripts
{
    public abstract class ProjectileModel : IProjectileModel
    {
        private readonly ReactiveProperty<bool> _isActive = new(false);

        private string _name;
        private Sprite _sprite;
        private ProjectileDamage _damage;
        private ProjectileSpeed _speed;

        public string Name => _name;
        public Sprite Sprite => _sprite;
        public int Damage => _damage?.Value ?? 0;
        public float Speed => _speed?.Value ?? 0f;

        public bool IsActive => _isActive.Value;
        public IReadOnlyReactiveProperty<bool> IsActiveRx => _isActive;

        protected ProjectileModel(string name, Sprite sprite, ProjectileDamage damage, ProjectileSpeed speed)
        {
            _name = name;
            _sprite = sprite;
            _damage = damage;
            _speed = speed;
        }

        public void Activate()
        {
            _isActive.Value = true;
            OnActivated();
        }

        public void Deactivate()
        {
            _isActive.Value = false;
            OnDeactivated();
        }

        public void SetDamage(ProjectileDamage damage) => _damage = damage;
        public void SetSpeed(ProjectileSpeed speed) => _speed = speed;

        // Hooks for derived models (e.g., clear counters on reuse later)
        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }
    }
}
