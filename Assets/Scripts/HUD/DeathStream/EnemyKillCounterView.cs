using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyKillCounterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string format = "Kills: {0}";

        private void Awake()
        {
            if (!label) label = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetCount(int count)
        {
            if (!label) return;
            label.text = string.Format(format, count);
        }
    }
}