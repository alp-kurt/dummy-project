using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Game/Waves/Multi-Entry Wave Config", fileName = "WaveConfig")]
    public sealed class WaveConfig : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public EnemyStats stats;           // Which enemy to spawn
            [Min(0)] public int countPerWave = 1; // How many per wave
        }

        [Header("What to spawn (per wave)")]
        public List<Entry> entries = new List<Entry>();

        [Header("When")]
        [Min(0.05f)] public float waveIntervalSeconds = 3f; // time between waves

        [Header("Where")]
        public float offscreenPadding = 2f; // spawn outside camera by this margin
    }
}
