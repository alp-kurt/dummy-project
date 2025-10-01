using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyKillCounterView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI label;

        [Tooltip("Display format. Must include {0} placeholder for the kill count.")]
        [SerializeField] private string format = "Kills: {0}";

        [Header("FX")]
        [Tooltip("How much to scale up on update (0.2 = +20%).")]
        [SerializeField, Min(0f)] private float bumpAmount = 0.2f;

        [Tooltip("Total duration of the bump (seconds).")]
        [SerializeField, Min(0.02f)] private float totalDuration = 0.18f;

        [Tooltip("Optional quick color flash tint on update; alpha preserved.")]
        [SerializeField] private Color hitTint = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField, Min(0f)] private float tintIn = 0.04f;
        [SerializeField, Min(0f)] private float tintOut = 0.08f;

        private Tween _scaleTw, _colorTw;
        private Color _baseColor;
        private Vector3 _baseScale = Vector3.one;
        private Transform _target;

        private void Awake()
        {
            if (!label) label = GetComponentInChildren<TextMeshProUGUI>(true);
            _target = label ? label.transform : transform;

            if (label) _baseColor = label.color;
            if (_target) _baseScale = _target.localScale;
        }

        private void OnDisable()
        {
            _scaleTw?.Kill(true);
            _colorTw?.Kill(true);

            if (_target) _target.localScale = _baseScale;
            if (label) label.color = _baseColor;
        }

        /// <summary>
        /// Renders the count and plays a safe, non-cumulative bump.
        /// </summary>
        public void SetCount(int count)
        {
            if (!label) return;

            label.text = string.Format(format, count);

            if (_target)
            {
                // Kill previous, reset to baseline, then run a bounded bump (no accumulation)
                _scaleTw?.Kill(false);
                _target.localScale = _baseScale;

                float up = totalDuration * 0.45f;
                float down = totalDuration - up;
                Vector3 peak = _baseScale * (1f + bumpAmount);

                _scaleTw = DOTween.Sequence()
                    .Append(_target.DOScale(peak, up).SetEase(Ease.OutQuad))
                    .Append(_target.DOScale(_baseScale, down).SetEase(Ease.OutBack))
                    .SetUpdate(false); // default Update loop
            }

            // Optional color flash (non-cumulative)
            if (label)
            {
                _colorTw?.Kill(false);
                if (hitTint != default)
                {
                    var target = new Color(hitTint.r, hitTint.g, hitTint.b, _baseColor.a);
                    _colorTw = label.DOColor(target, tintIn)
                                    .OnComplete(() => _colorTw = label.DOColor(_baseColor, tintOut))
                                    .OnKill(() => { if (label) label.color = _baseColor; });
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!label) label = GetComponentInChildren<TextMeshProUGUI>(true);
        }
#endif
    }
}
