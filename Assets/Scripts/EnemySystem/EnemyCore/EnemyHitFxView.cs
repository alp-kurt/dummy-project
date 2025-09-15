using UnityEngine;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHitFxView : MonoBehaviour, IPooledViewModule
    {
        [Header("Refs")]
        [Tooltip("If left empty, will search in children on Awake/OnValidate.")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Squash/Stretch")]
        [Tooltip("Target minimum scale (0.6–1.0).")]
        [Range(0.6f, 1f)] [SerializeField] private float _scaleMin = 0.9f;

        [Tooltip("Duration of the squash-in.")]
        [SerializeField, Min(0f)] private float _scaleIn = 0.06f;

        [Tooltip("Duration of the stretch-out.")]
        [SerializeField, Min(0f)] private float _scaleOut = 0.10f;

        [SerializeField] private Ease _easeIn = Ease.OutCubic;
        [SerializeField] private Ease _easeOut = Ease.OutBack;

        private Vector3 _baseScale = Vector3.one;

        private Tween _scaleTween;

        private void Awake()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        public void OnSpawn()
        {
            if (!_spriteRenderer) return;
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
            _scaleTween?.Kill(true);
        }

        private void RestoreBase()
        {
            if (!_spriteRenderer) return;
            _spriteRenderer.transform.localScale = _baseScale;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            _scaleIn = Mathf.Max(0f, _scaleIn);
            _scaleOut = Mathf.Max(0f, _scaleOut);

            if (!_spriteRenderer)
                Debug.LogWarning("[EnemyHitFxView] SpriteRenderer not assigned or found in children.", this);
        }
#endif
    }
}
