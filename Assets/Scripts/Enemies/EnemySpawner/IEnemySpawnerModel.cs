using System;
using UniRx;

namespace Scripts
{
    public interface IEnemyWaveSpawnerModel
    {
        WaveConfig Wave { get; }
        int RandomSeed { get; }

        IReadOnlyReactiveProperty<int> WaveIndex { get; } // 0-based
        TimeSpan WaveInterval { get; }
        float OffscreenPadding { get; }

        void AdvanceWave();
    }
}
