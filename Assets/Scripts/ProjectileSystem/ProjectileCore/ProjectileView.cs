using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileView : PooledView
    {
        [Header("Visuals")]
        [Tooltip("If left empty, will search in children on Awake/OnValidate.")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Collision")]
        [Tooltip("Layers considered valid hit targets for this projectile.")]
        [SerializeField] private LayerMask _targetMask;

        private readonly Subject<IDamageable> _hitTargets = new();
        public IObservable<IDamageable> HitTargets => _hitTargets;

        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private Transform _cachedTransform;
        public Transform CachedTransform => _cachedTransform ? _cachedTransform : (_cachedTransform = transform);

        private void Awake()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer) _spriteRenderer.sprite = sprite;
        }

        public void SetActive(bool active) => gameObject.SetActive(active);
        public void SetPosition(Vector3 position) => CachedTransform.position = position;
        public void Move(Vector3 delta) => CachedTransform.position += delta;

        public void ResetForPool()
        {
            CachedTransform.rotation = Quaternion.identity;
            CachedTransform.localScale = Vector3.one;
        }

        public override void OnRent()
        {
            base.OnRent();
            SetActive(true);
            ResetForPool();
        }

        public override void OnRelease()
        {
            base.OnRelease();
            SetActive(false);
        }

        private bool IsInTargetMask(int layer) => (_targetMask.value & (1 << layer)) != 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Layer filter first
            if (!IsInTargetMask(other.gameObject.layer)) return;

            // Same object fast path
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                _hitTargets.OnNext(damageable);
                _signalBus.Fire(new ProjectileHitSignal { View = this, Target = damageable });
                return;
            }

            // Parent fallback (common when colliders are on child objects)
            var dmgFromParent = other.GetComponentInParent<IDamageable>();
            if (dmgFromParent != null)
            {
                _hitTargets.OnNext(dmgFromParent);
                _signalBus.Fire(new ProjectileHitSignal { View = this, Target = dmgFromParent });
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

            var col = GetComponent<Collider2D>();
            if (!col)
            {
                Debug.LogWarning("[ProjectileView] Missing Collider2D. Add one for trigger hits to work.", this);
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning("[ProjectileView] Collider2D is not set as Trigger. OnTriggerEnter2D will not fire.", this);
            }

            if (_targetMask.value == 0)
            {
                Debug.LogWarning("[ProjectileView] Target mask is empty. Select at least one layer.", this);
            }
        }
#endif
    }
}
