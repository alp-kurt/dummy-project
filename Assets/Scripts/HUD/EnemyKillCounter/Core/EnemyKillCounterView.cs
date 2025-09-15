using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyKillCounterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        [Tooltip("Display format. Must include {0} placeholder for the kill count.")]
        [SerializeField] private string format = "Kills: {0}";

        private void Awake()
        {
            if (!label) label = GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Renders the count using the configured <see cref="format"/>.
        /// No-op if label is not assigned and cannot be auto-resolved.
        /// </summary>
        public void SetCount(int count)
        {
            if (!label) return;
            label.text = string.Format(format, count);
        }
    }
}
