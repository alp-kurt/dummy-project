using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : MonoBehaviour
    {
        [Header("Render / Movement")]
        [SerializeField] private SpriteRenderer m_spriteRenderer;
        [SerializeField] private Rigidbody2D m_rigidbody2D;
        [SerializeField] private Transform m_root;

        [Header("Visibility (renderer lives on this object)")]
        private readonly Subject<bool> m_visibilityChanged = new Subject<bool>();
        public IObservable<bool> VisibilityChanged => m_visibilityChanged;

        [Header("Health UI (World-Space Slider)")]
        [SerializeField] private Slider m_healthBar;                 // optional: auto-found if null
        [SerializeField, Range(0f, 1f)] private float m_healthTweenSeconds = 0.2f;
        [SerializeField] private bool m_hideHealthBarWhenFull = false;
        [SerializeField] private CanvasGroup m_healthCanvasGroup;    // optional fade; auto-found if null

        public Vector3 Position => (m_root != null ? m_root : transform).position;

        private void Awake()
        {
            if (m_root == null) m_root = transform;
            if (m_spriteRenderer == null) m_spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

            // Auto-find health UI pieces if not assigned
            if (m_healthBar == null) m_healthBar = GetComponentInChildren<Slider>(true);
            if (m_healthCanvasGroup == null && m_healthBar != null)
                m_healthCanvasGroup = m_healthBar.GetComponentInParent<CanvasGroup>();

            if (m_healthBar != null)
            {
                m_healthBar.minValue = 0f;
                m_healthBar.maxValue = 1f;
                m_healthBar.value = 1f; // start full
                if (m_hideHealthBarWhenFull && m_healthCanvasGroup != null)
                    m_healthCanvasGroup.alpha = 0f; // hidden while full
            }
        }

        #region Visibility hooks
        private void OnBecameVisible() => m_visibilityChanged.OnNext(true);
        private void OnBecameInvisible() => m_visibilityChanged.OnNext(false);
        #endregion

        // --- UI ---

        /// <summary>Update the enemy health bar with a normalized [0..1] value.</summary>
        public void UpdateHealth(float current01)
        {
            if (m_healthBar == null) return;

            float clamped = Mathf.Clamp01(current01);
            m_healthBar.DOValue(clamped, m_healthTweenSeconds).SetEase(Ease.OutSine);

            if (m_hideHealthBarWhenFull && m_healthCanvasGroup != null)
            {
                // Fade in when below 1, fade out when full
                float targetAlpha = clamped < 0.999f ? 1f : 0f;
                m_healthCanvasGroup.DOFade(targetAlpha, 0.15f);
            }
        }

        public void SetHealthVisible(bool visible)
        {
            if (m_healthBar == null || m_healthCanvasGroup == null) return;
            m_healthCanvasGroup.alpha = visible ? 1f : 0f;
        }

        // --- Visuals / Movement ---

        public void SetVisual(Sprite sprite, float scale)
        {
            if (m_spriteRenderer != null) m_spriteRenderer.sprite = sprite;
            var s = Mathf.Max(0.01f, scale);
            (m_root != null ? m_root : transform).localScale = Vector3.one * s;
        }

        public void ApplyVelocity(Vector2 velocity)
        {
            if (m_rigidbody2D != null)
                m_rigidbody2D.velocity = velocity;
            else
                (m_root != null ? m_root : transform).position += (Vector3)(velocity * Time.deltaTime);
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
