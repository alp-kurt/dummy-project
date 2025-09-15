using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Game/Waves/Multi-Enemy Wave Config", fileName = "WaveConfig")]
    public sealed class EnemyWaveConfig : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            [Header("Enemy & Count")]
            [Tooltip("Which enemy stats to spawn for this entry.")]
            [SerializeField] private EnemyStats _stats;

            [Tooltip("How many instances to spawn each wave. 0 = skip this entry.")]
            [SerializeField, Min(0)] private int _countPerWave = 1;

            public EnemyStats Stats => _stats;
            public int CountPerWave => _countPerWave;
        }

        [Header("What to spawn (per wave)")]
        [Tooltip("List of enemy entries spawned each wave (processed in order).")]
        [SerializeField] private List<Entry> _entries = new();

        [Header("When")]
        [Tooltip("Seconds to wait between successive waves (>= 0.05).")]
        [SerializeField, Min(0.05f)] private float _waveIntervalSeconds = 3f;

        [Header("Where")]
        [Tooltip("World-space padding outside the camera where enemies spawn.")]
        [SerializeField, Min(0f)] private float _offscreenPadding = 2f;

        public IReadOnlyList<Entry> Entries => _entries;
        public float WaveIntervalSeconds => _waveIntervalSeconds;
        public float OffscreenPadding => _offscreenPadding;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_entries == null) _entries = new List<Entry>();
            if (_waveIntervalSeconds < 0.05f) _waveIntervalSeconds = 0.05f;
            if (_offscreenPadding < 0f) _offscreenPadding = 0f;

            // Tiny per-entry validation with clear context
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null)
                {
                    Debug.LogWarning($"[WaveConfig] '{name}' Entry[{i}] is null.", this);
                    continue;
                }
                if (e.Stats == null)
                    Debug.LogWarning($"[WaveConfig] '{name}' Entry[{i}] has no EnemyStats assigned.", this);

                if (e.CountPerWave < 0)
                    Debug.LogWarning($"[WaveConfig] '{name}' Entry[{i}] CountPerWave < 0 (will be treated as 0).", this);
            }
        }
#endif
    }
}
