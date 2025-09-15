using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHealthBarView : MonoBehaviour, IPooledViewModule
    {
        [SerializeField] private Slider m_slider;
        [SerializeField, Range(0f, 1f)] private float m_valueTween = 0.2f;

        private Tween m_valueTw, m_fadeTw;

        public void OnSpawn()
        {
            KillTweens();
            if (m_slider) m_slider.value = 1f;
        }

        public void OnDespawn()
        {
            KillTweens();
            if (m_slider) m_slider.value = 1f;
        }

        public void SetVisible(bool visible)
        {
            m_fadeTw?.Kill(true);
        }

        public void SetHealth01(float value01)
        {
            if (!m_slider) return;

            m_valueTw?.Kill(true);
            m_valueTw = m_slider.DOValue(Mathf.Clamp01(value01), m_valueTween).SetUpdate(true);
        }

        private void KillTweens()
        {
            m_valueTw?.Kill(true);
            m_fadeTw?.Kill(true);
        }
    }
}
