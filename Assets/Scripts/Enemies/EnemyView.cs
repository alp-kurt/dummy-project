using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer m_spriteRenderer;
        [SerializeField] private Rigidbody2D m_rigidbody2D; // optional; if null we will move transform
        [SerializeField] private Transform m_root; // scale & position pivot; default to this.transform

        private readonly Subject<bool> m_visibilityChanged = new Subject<bool>();

        public IObservable<bool> VisibilityChanged => m_visibilityChanged;
        public Vector3 Position => (m_root != null ? m_root : transform).position;

        private void Awake()
        {
            if (m_root == null) m_root = transform;
        }

        private void OnBecameVisible()
        {
            m_visibilityChanged.OnNext(true);
        }

        private void OnBecameInvisible()
        {
            m_visibilityChanged.OnNext(false);
        }

        public void SetVisual(Sprite sprite, float scale)
        {
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.sprite = sprite;
            }
            var s = Mathf.Max(0.01f, scale);
            (m_root != null ? m_root : transform).localScale = Vector3.one * s;
        }

        public void ApplyVelocity(Vector2 velocity)
        {
            if (m_rigidbody2D != null)
            {
                m_rigidbody2D.velocity = velocity;
            }
            else
            {
                // fallback transform move
                (m_root != null ? m_root : transform).position += (Vector3)(velocity * Time.deltaTime);
            }
        }

        public void Stop()
        {
            if (m_rigidbody2D != null) m_rigidbody2D.velocity = Vector2.zero;
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}
