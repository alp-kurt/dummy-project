using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [Serializable]
    public sealed class WaveEntry
    {
        public EnemyStats stats;
        [Min(0f)] public float weight = 1f; // spawn likelihood
    }

    [CreateAssetMenu(menuName = "Game/Waves/Wave Config", fileName = "WaveConfig")]
    public sealed class WaveConfig : ScriptableObject
    {
        [Header("Composition")]
        public List<WaveEntry> entries = new List<WaveEntry>();

        [Header("Spawn Tuning")]
        [Min(0.05f)] public float spawnIntervalSeconds = 1.0f; // how often to spawn
        [Min(1)] public int densityPerTick = 1;                 // how many per tick

        [Header("Spawn Placement")]
        [Min(0f)] public float offscreenPadding = 2.0f;

        public EnemyStats PickRandomStats(System.Random rng)
        {
            if (entries == null || entries.Count == 0) return null;

            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
                total += Mathf.Max(0f, entries[i].weight);

            if (total <= 0f) return entries[0].stats;

            float r = (float)(rng.NextDouble() * total);
            float cumulative = 0f;

            for (int i = 0; i < entries.Count; i++)
            {
                cumulative += Mathf.Max(0f, entries[i].weight);
                if (r <= cumulative) return entries[i].stats;
            }
            return entries[entries.Count - 1].stats;
        }
    }
}
