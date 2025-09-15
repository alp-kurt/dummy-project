using System;
using UniRx;

namespace Scripts
{
    /// <summary>
    /// LiveOps-friendly state/tunables for the enemy wave spawner.
    /// </summary>
    public interface IEnemyWaveSpawnerModel
    {
        EnemyWaveConfig Wave { get; }
        int RandomSeed { get; }

        IReadOnlyReactiveProperty<int> WaveIndex { get; } // 0-based
        TimeSpan WaveInterval { get; }                    // per-wave pause
        float OffscreenPadding { get; }                   // spawn just outside the camera

        void AdvanceWave();
    }
}
