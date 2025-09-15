using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHealthBarView : MonoBehaviour, IPooledViewModule
    {
        [SerializeField] private Slider _slider;
        [SerializeField, Range(0f, 1f)] private float _valueTween = 0.2f;

        private Tween _valueTw, _fadeTw;

        public void OnSpawn()
        {
            KillTweens();
            if (_slider) _slider.value = 1f;
        }

        public void OnDespawn()
        {
            KillTweens();
            if (_slider) _slider.value = 1f;
        }

        public void SetVisible(bool visible)
        {
            _fadeTw?.Kill(true);
        }

        public void SetHealth01(float value01)
        {
            if (!_slider) return;

            _valueTw?.Kill(true);
            _valueTw = _slider.DOValue(Mathf.Clamp01(value01), _valueTween).SetUpdate(true);
        }

        private void KillTweens()
        {
            _valueTw?.Kill(true);
            _fadeTw?.Kill(true);
        }
    }
}
