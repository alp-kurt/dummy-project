using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : PooledView
    {
        [SerializeField] private SpriteRenderer m_spriteRenderer;
        [SerializeField] private Rigidbody2D m_rigidbody2D;
        [SerializeField] private Transform m_root;
        [SerializeField] private int m_contactDamage;

        // Optional plug-ins (assign in prefab or fetched via GetComponentInChildren)
        [SerializeField] private EnemyHitFxView m_hitFxView;
        [SerializeField] private EnemyHealthBarView m_healthBarView;

        private readonly Subject<bool> m_visibilityChanged = new();
        public IObservable<bool> VisibilityChanged => m_visibilityChanged;

        public EnemyHitFxView HitFxView => m_hitFxView;
        public EnemyHealthBarView HealthBarView => m_healthBarView;

        public int ContactDamage => m_contactDamage;
        public void SetContactDamage(int value) => m_contactDamage = Mathf.Max(0, value);

        public Vector3 Position => (m_root != null ? m_root : transform).position;

        public void SetVisual(Sprite sprite, float scale = 1f)
        {
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.sprite = sprite;
                m_spriteRenderer.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
                // let modules read base color/scale if they need (they’ll cache on OnSpawn)
            }
        }

        public void ApplyVelocityFixed(Vector2 velocity, float fixedDt)
        {
            var delta = velocity * Mathf.Max(0f, fixedDt);
            if (m_rigidbody2D != null) m_rigidbody2D.MovePosition(m_rigidbody2D.position + delta);
            else (m_root != null ? m_root : transform).position += (Vector3)delta;
        }

        public void Stop()
        {
            if (m_rigidbody2D != null) m_rigidbody2D.velocity = Vector2.zero;
        }

        private void OnBecameVisible() => m_visibilityChanged.OnNext(true);
        private void OnBecameInvisible() => m_visibilityChanged.OnNext(false);

        public void SetActive(bool value) => gameObject.SetActive(value);

        // Helpers for presenter: expose modules even if not wired in inspector
        public void EnsureModulesCached()
        {
            if (!m_hitFxView) m_hitFxView = GetComponentInChildren<EnemyHitFxView>(true);
            if (!m_healthBarView) m_healthBarView = GetComponentInChildren<EnemyHealthBarView>(true);
        }
    }
}
