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

        public Transform CachedTransform => transform;

        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = sprite;
        }

        public void SetActive(bool active) => gameObject.SetActive(active);
        public void SetPosition(Vector3 position) => transform.position = position;
        public void Move(Vector3 delta) => transform.position += delta;

        public void ResetForPool()
        {
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
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
            // Subscribers are disposed by presenter; Subject remains for next rent (no allocations).
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

            // Alloc-free capability check
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                hitTargets.OnNext(damageable);
                Debug.Log("EnemyHit");
            }
        }
    }
}