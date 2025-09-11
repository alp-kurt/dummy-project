using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthView : MonoBehaviour
    {
        [SerializeField] private Slider healthBar;
        [SerializeField, Range(0f, 1f)] private float tweenSeconds = 0.2f;

        private void Awake()
        {
            if (healthBar == null)
            {
                healthBar = GetComponentInChildren<Slider>();
                if (healthBar == null)
                {
                    Debug.LogWarning("[PlayerHealthView] Slider not assigned/found.", this);
                }
            }
        }

        /// <summary>Updates the slider with normalized health [0..1].</summary>
        public void SetHealth01(float value01)
        {
            if (healthBar == null) return;
            float v = Mathf.Clamp01(value01);
            healthBar.DOValue(v, tweenSeconds).SetEase(Ease.OutSine);
        }
    }
}
