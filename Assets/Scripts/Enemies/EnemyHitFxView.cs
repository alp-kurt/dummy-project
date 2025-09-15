using UnityEngine;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHitFxView : MonoBehaviour, IPooledViewModule
    {
        [Header("Refs")]
        [SerializeField] private SpriteRenderer m_spriteRenderer;

        [Header("Flash")]
        [SerializeField] private float m_flashIn = 0.05f;
        [SerializeField] private float m_flashOut = 0.12f;

        [Header("Squash/Woogle")]
        [Range(0.6f, 1f)] [SerializeField] private float m_scaleMin = 0.9f;
        [SerializeField] private float m_scaleIn = 0.06f;
        [SerializeField] private float m_scaleOut = 0.10f;
        [SerializeField] private Ease m_easeIn = Ease.OutCubic;
        [SerializeField] private Ease m_easeOut = Ease.OutBack;

        private Color m_baseColor = Color.white;
        private Vector3 m_baseScale = Vector3.one;

        private Tween m_flashTween, m_scaleTween;

        public void OnSpawn()
        {
            if (!m_spriteRenderer) return;
            m_baseColor = m_spriteRenderer.color;
            m_baseScale = m_spriteRenderer.transform.localScale;
            KillTweens();
            RestoreBase();
        }

        public void OnDespawn()
        {
            KillTweens();
            RestoreBase();
        }

        public void PlayOnHit()
        {
            if (!m_spriteRenderer) return;

            // flash
            m_flashTween?.Kill(true);
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(m_spriteRenderer.DOColor(Color.white, m_flashIn));
            seq.Append(m_spriteRenderer.DOColor(m_baseColor, m_flashOut));
            m_flashTween = seq;

            // squash
            m_scaleTween?.Kill(true);
            var tr = m_spriteRenderer.transform;
            var target = m_baseScale * Mathf.Clamp01(m_scaleMin);
            var s = DOTween.Sequence().SetUpdate(true);
            s.Append(tr.DOScale(target, m_scaleIn).SetEase(m_easeIn));
            s.Append(tr.DOScale(m_baseScale, m_scaleOut).SetEase(m_easeOut));
            m_scaleTween = s;
        }

        private void KillTweens()
        {
            m_flashTween?.Kill(true);
            m_scaleTween?.Kill(true);
        }

        private void RestoreBase()
        {
            if (!m_spriteRenderer) return;
            m_spriteRenderer.color = m_baseColor;
            m_spriteRenderer.transform.localScale = m_baseScale;
        }
    }
}
