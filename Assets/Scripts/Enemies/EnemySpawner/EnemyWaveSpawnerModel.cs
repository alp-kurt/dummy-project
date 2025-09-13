using System;
using UniRx;
using UnityEngine;
namespace Scripts
{
    public sealed class EnemyWaveSpawnerModel : IEnemyWaveSpawnerModel
    {
        private readonly ReactiveProperty<int> m_waveIndex = new ReactiveProperty<int>(0);

        public WaveConfig Wave { get; }
        public int RandomSeed { get; }
        public IReadOnlyReactiveProperty<int> WaveIndex => m_waveIndex;

        public TimeSpan WaveInterval => TimeSpan.FromSeconds(Mathf.Max(0.05f, Wave.waveIntervalSeconds));
        public float OffscreenPadding => Wave.offscreenPadding;

        public EnemyWaveSpawnerModel(WaveConfig wave, int randomSeed)
        {
            Wave = wave;
            RandomSeed = randomSeed;
        }

        public void AdvanceWave()
        {
            m_waveIndex.Value = m_waveIndex.Value + 1;
        }
    }
}