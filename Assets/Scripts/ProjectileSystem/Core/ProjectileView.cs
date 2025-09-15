using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public class ProjectileView : PooledView
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Collision")]
        [SerializeField] private LayerMask targetMask;

        private readonly Subject<IDamageable> hitTargets = new();
        public IObservable<IDamageable> HitTargets => hitTargets;

        private Transform _cachedTransform;
        public Transform CachedTransform => _cachedTransform ? _cachedTransform : (_cachedTransform = transform);

        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = sprite;
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
            // Keep the subject alive for pooled subscribers; just ensure no lingering observers misbehave.
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                hitTargets.OnNext(damageable);
            }
        }
    }
}
