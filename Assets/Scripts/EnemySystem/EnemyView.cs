using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : PooledView, IDamageable
    {
        [Header("Core Refs")]
        [Tooltip("Primary renderer for this enemy (can be on a child).")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Tooltip("Optional. If missing, movement falls back to setting Transform position.")]
        [SerializeField] private Rigidbody2D _rigidbody2D;

        [Tooltip("Optional visual root (scale/pos). Defaults to this Transform.")]
        [SerializeField] private Transform _root;

        private int _contactDamage = 1;

        [Header("Optional Modules")]
        [Tooltip("Optional. Found in children if not assigned.")]
        [SerializeField] private EnemyHealthBarView _healthBarView;

        private readonly Subject<bool> _visibilityChanged = new();
        public IObservable<bool> VisibilityChanged => _visibilityChanged;

        private bool _isVisible;
        public bool IsVisible => _isVisible;

        public EnemyHealthBarView HealthBarView => _healthBarView;

        public int ContactDamage => _contactDamage;

        public void SetContactDamage(int value) => _contactDamage = Mathf.Max(0, value);

        public Vector3 Position => (_root != null ? _root : transform).position;

        // Model attachment & damage forwarding
        private IEnemyModel _model;

        /// <summary>Presenter calls once per spawn.</summary>
        public void AttachModel(IEnemyModel model) => _model = model;

        /// <summary>Expose per-enemy model so child views (e.g., health bar) can bind reactively.</summary>
        public IEnemyModel Model => _model;   // <-- added

        public void ReceiveDamage(int amount) => _model?.ReceiveDamage(amount);

        public void SetVisual(Sprite sprite, float scale = 1f)
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        }

        public void ApplyVelocityFixed(Vector2 velocity, float fixedDt)
        {
            var delta = velocity * Mathf.Max(0f, fixedDt);
            if (_rigidbody2D != null) _rigidbody2D.MovePosition(_rigidbody2D.position + delta);
            else (_root != null ? _root : transform).position += (Vector3)delta;
        }

        public void Stop()
        {
            if (_rigidbody2D != null) _rigidbody2D.linearVelocity = Vector2.zero;
        }

        private void OnBecameVisible()
        {
            _isVisible = true;
            _visibilityChanged.OnNext(true);
        }

        private void OnBecameInvisible()
        {
            _isVisible = false;
            _visibilityChanged.OnNext(false);
        }

        public bool IsActive => gameObject.activeSelf;

        public void SetActive(bool value)
        {
            if (gameObject.activeSelf == value) return;
            gameObject.SetActive(value);
        }

        public void EnsureModulesCached()
        {
            if (!_healthBarView) _healthBarView = GetComponentInChildren<EnemyHealthBarView>(true);
        }

        private void Awake()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (!_rigidbody2D) _rigidbody2D = GetComponentInChildren<Rigidbody2D>(true) ?? GetComponent<Rigidbody2D>();
            if (!_root) _root = transform;
            EnsureModulesCached();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (!_rigidbody2D) _rigidbody2D = GetComponentInChildren<Rigidbody2D>(true) ?? GetComponent<Rigidbody2D>();
            if (!_root) _root = transform;

            if (!_spriteRenderer)
                Debug.LogWarning("[EnemyView] SpriteRenderer not assigned or found in children.", this);

            if (!GetComponentInChildren<Collider2D>(true))
                Debug.LogWarning("[EnemyView] No Collider2D found; enemy won’t collide with player/projectiles.", this);
        }
#endif
    }
}
