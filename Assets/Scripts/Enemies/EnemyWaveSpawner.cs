using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Spawns waves defined by WaveConfig. Each wave spawns the exact countPerWave
    /// for each entry. No auto-despawn; recycling happens only when gameplay triggers it.
    /// </summary>
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private Camera cam;
        [SerializeField] private WaveConfig wave;

        [Header("Randomness")]
        [Tooltip("0 = non-deterministic; non-zero = deterministic spawn positions")]
        [SerializeField] private int randomSeed = 12345;

        private IEnemyFactory m_enemyFactory;
        private System.Random m_rng;
        private CancellationTokenSource m_cts;

        [Inject]
        private void Construct(IEnemyFactory enemyFactory)
        {
            m_enemyFactory = enemyFactory;
        }

        private void OnEnable()
        {
            if (!cam) cam = Camera.main;

            if (wave == null)
            {
                Debug.LogError("EnemyWaveSpawner: WaveConfig is missing.");
                enabled = false;
                return;
            }

            m_rng = (randomSeed == 0) ? new System.Random() : new System.Random(randomSeed);
            m_cts = new CancellationTokenSource();
            RunLoop(m_cts.Token).Forget();
        }

        private void OnDisable()
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_cts = null;
        }

        private async UniTaskVoid RunLoop(CancellationToken token)
        {
            var delay = TimeSpan.FromSeconds(Mathf.Max(0.05f, wave.waveIntervalSeconds));

            while (!token.IsCancellationRequested)
            {
                // For each wave, spawn each entry's fixed count
                if (wave.entries != null)
                {
                    for (int e = 0; e < wave.entries.Count && !token.IsCancellationRequested; e++)
                    {
                        var entry = wave.entries[e];
                        if (entry == null || entry.stats == null || entry.countPerWave <= 0)
                            continue;

                        for (int i = 0; i < entry.countPerWave && !token.IsCancellationRequested; i++)
                        {
                            Vector3 pos = GetOffScreenPosition(cam, wave.offscreenPadding, m_rng);

                            IEnemyHandle handle = null;
                            try
                            {
                                handle = m_enemyFactory.Create(entry.stats, pos);
                            }
                            catch (InvalidOperationException)
                            {
                                // Pool is currently full; skip this spawn safely.
                                continue;
                            }

                            // Release to pool only when your gameplay (Presenter/Model) signals it.
                            handle.ReturnedToPool
                                  .Take(1)
                                  .Subscribe(_ => handle.Release())
                                  .AddTo(handle.View);

                            handle.Spawn();
                        }
                    }
                }

                await UniTask.Delay(delay, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        private static Vector3 GetOffScreenPosition(Camera camera, float padding, System.Random rng)
        {
            float z = Mathf.Abs(camera.transform.position.z);
            Vector3 bl = camera.ViewportToWorldPoint(new Vector3(0f, 0f, z));
            Vector3 tr = camera.ViewportToWorldPoint(new Vector3(1f, 1f, z));

            float left = Mathf.Min(bl.x, tr.x);
            float right = Mathf.Max(bl.x, tr.x);
            float bottom = Mathf.Min(bl.y, tr.y);
            float top = Mathf.Max(bl.y, tr.y);

            int side = rng.Next(0, 4);
            float x, y;
            switch (side)
            {
                case 0: x = left - padding; y = Mathf.Lerp(bottom, top, (float)rng.NextDouble()); break;
                case 1: x = right + padding; y = Mathf.Lerp(bottom, top, (float)rng.NextDouble()); break;
                case 2: x = Mathf.Lerp(left, right, (float)rng.NextDouble()); y = bottom - padding; break;
                default: x = Mathf.Lerp(left, right, (float)rng.NextDouble()); y = top + padding; break;
            }
            return new Vector3(x, y, 0f);
        }
    }
}
