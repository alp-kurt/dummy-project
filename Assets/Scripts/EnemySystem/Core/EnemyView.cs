using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : PooledView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _root;
        [SerializeField] private int _contactDamage;

        // Optional plug-ins (assign in prefab or fetched via GetComponentInChildren)
        [SerializeField] private EnemyHitFxView _hitFxView;
        [SerializeField] private EnemyHealthBarView _healthBarView;

        private readonly Subject<bool> _visibilityChanged = new();
        public IObservable<bool> VisibilityChanged => _visibilityChanged;

        public EnemyHitFxView HitFxView => _hitFxView;
        public EnemyHealthBarView HealthBarView => _healthBarView;

        public int ContactDamage => _contactDamage;
        public void SetContactDamage(int value) => _contactDamage = Mathf.Max(0, value);

        public Vector3 Position => (_root != null ? _root : transform).position;

        public void SetVisual(Sprite sprite, float scale = 1f)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprite;
                _spriteRenderer.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
                // let modules read base color/scale if they need (they’ll cache on OnSpawn)
            }
        }

        public void ApplyVelocityFixed(Vector2 velocity, float fixedDt)
        {
            var delta = velocity * Mathf.Max(0f, fixedDt);
            if (_rigidbody2D != null) _rigidbody2D.MovePosition(_rigidbody2D.position + delta);
            else (_root != null ? _root : transform).position += (Vector3)delta;
        }

        public void Stop()
        {
            if (_rigidbody2D != null) _rigidbody2D.velocity = Vector2.zero;
        }

        private void OnBecameVisible() => _visibilityChanged.OnNext(true);
        private void OnBecameInvisible() => _visibilityChanged.OnNext(false);

        public void SetActive(bool value) => gameObject.SetActive(value);

        // Helpers for presenter: expose modules even if not wired in inspector
        public void EnsureModulesCached()
        {
            if (!_hitFxView) _hitFxView = GetComponentInChildren<EnemyHitFxView>(true);
            if (!_healthBarView) _healthBarView = GetComponentInChildren<EnemyHealthBarView>(true);
        }
    }
}
