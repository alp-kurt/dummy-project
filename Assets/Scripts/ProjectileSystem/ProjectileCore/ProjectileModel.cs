using UniRx;
using UnityEngine;

namespace Scripts
{
    public abstract class ProjectileModel : IProjectileModel
    {
        private readonly ReactiveProperty<bool> _isActive = new(false);

        private readonly string _name;
        private readonly Sprite _sprite;
        private int _damage;
        private float _speed;

        public string Name => _name;
        public Sprite Sprite => _sprite;
        public int Damage => _damage;
        public float Speed => _speed;

        public bool IsActive => _isActive.Value;
        public IReadOnlyReactiveProperty<bool> IsActiveRx => _isActive;

        protected ProjectileModel(string name, Sprite sprite, int damage, float speed)
        {
            _name = name; _sprite = sprite; _damage = Mathf.Max(0, damage); _speed = Mathf.Max(0f, speed);
        }

        public void Activate() { _isActive.Value = true; OnActivated(); }
        public void Deactivate() { _isActive.Value = false; OnDeactivated(); }

        public void SetStats(int damage, float speed)
        {
            _damage = Mathf.Max(0, damage);
            _speed = Mathf.Max(0f, speed);
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }
    }
}
