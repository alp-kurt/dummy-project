using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHealthBarView : MonoBehaviour, IPooledViewModule
    {
        [Header("Refs")]
        [Tooltip("If left empty, will search in children on Awake/OnValidate.")]
        [SerializeField] private Slider _slider;

        [Tooltip("Optional. If present, SetVisible() will animate alpha; else we toggle GameObject.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")]
        [Tooltip("Time to tween health value (0..1 seconds).")]
        [SerializeField, Range(0f, 1f)] private float _valueTween = 0.2f;

        private Tween _valueTw, _fadeTw;

        private void Awake()
        {
            if (!_slider) _slider = GetComponentInChildren<Slider>(true);
            if (!_canvasGroup) _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        public void OnSpawn()
        {
            KillTweens();
            if (_slider) _slider.value = 1f;
            SetVisible(true);
        }

        public void OnDespawn()
        {
            KillTweens();
            if (_slider) _slider.value = 1f;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _fadeTw?.Kill(true);

            if (_canvasGroup)
            {
                _fadeTw = _canvasGroup
                    .DOFade(visible ? 1f : 0f, 0.15f)
                    .SetUpdate(true);

                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
            else if (_slider)
            {
                _slider.gameObject.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        public void SetHealth01(float value01)
        {
            if (!_slider) return;

            _valueTw?.Kill(true);
            _valueTw = _slider
                .DOValue(Mathf.Clamp01(value01), _valueTween)
                .SetUpdate(true);
        }

        private void KillTweens()
        {
            _valueTw?.Kill(true);
            _fadeTw?.Kill(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_slider) _slider = GetComponentInChildren<Slider>(true);
            if (!_canvasGroup) _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            _valueTween = Mathf.Clamp01(_valueTween);

            if (!_slider)
                Debug.LogWarning("[EnemyHealthBarView] Slider not assigned or found in children.", this);
        }
#endif
    }
}
