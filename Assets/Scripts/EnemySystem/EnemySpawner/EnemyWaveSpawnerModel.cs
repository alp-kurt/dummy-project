using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyWaveSpawnerModel : IEnemyWaveSpawnerModel
    {
        private readonly ReactiveProperty<int> _waveIndex = new ReactiveProperty<int>(0);

        public EnemyWaveConfig Wave { get; }
        public int RandomSeed { get; }
        public IReadOnlyReactiveProperty<int> WaveIndex => _waveIndex;

        public TimeSpan WaveInterval => TimeSpan.FromSeconds(Mathf.Max(0.05f, Wave.WaveIntervalSeconds));
        public float OffscreenPadding => Wave.OffscreenPadding;

        public EnemyWaveSpawnerModel(EnemyWaveConfig wave, int randomSeed)
        {
            Wave = wave;
            RandomSeed = randomSeed;
        }

        public void AdvanceWave()
        {
            _waveIndex.Value = _waveIndex.Value + 1;
        }
    }
}
