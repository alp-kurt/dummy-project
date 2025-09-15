using UnityEngine;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHitFxView : MonoBehaviour, IPooledViewModule
    {
        [Header("Refs")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Flash")]
        [SerializeField] private float _flashIn = 0.05f;
        [SerializeField] private float _flashOut = 0.12f;

        [Header("Squash/Woogle")]
        [Range(0.6f, 1f)] [SerializeField] private float _scaleMin = 0.9f;
        [SerializeField] private float _scaleIn = 0.06f;
        [SerializeField] private float _scaleOut = 0.10f;
        [SerializeField] private Ease _easeIn = Ease.OutCubic;
        [SerializeField] private Ease _easeOut = Ease.OutBack;

        private Color _baseColor = Color.white;
        private Vector3 _baseScale = Vector3.one;

        private Tween _flashTween, _scaleTween;

        public void OnSpawn()
        {
            if (!_spriteRenderer) return;
            _baseColor = _spriteRenderer.color;
            _baseScale = _spriteRenderer.transform.localScale;
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
            if (!_spriteRenderer) return;

            // flash
            _flashTween?.Kill(true);
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(_spriteRenderer.DOColor(Color.white, _flashIn));
            seq.Append(_spriteRenderer.DOColor(_baseColor, _flashOut));
            _flashTween = seq;

            // squash
            _scaleTween?.Kill(true);
            var tr = _spriteRenderer.transform;
            var target = _baseScale * Mathf.Clamp01(_scaleMin);
            var s = DOTween.Sequence().SetUpdate(true);
            s.Append(tr.DOScale(target, _scaleIn).SetEase(_easeIn));
            s.Append(tr.DOScale(_baseScale, _scaleOut).SetEase(_easeOut));
            _scaleTween = s;
        }

        private void KillTweens()
        {
            _flashTween?.Kill(true);
            _scaleTween?.Kill(true);
        }

        private void RestoreBase()
        {
            if (!_spriteRenderer) return;
            _spriteRenderer.color = _baseColor;
            _spriteRenderer.transform.localScale = _baseScale;
        }
    }
}
