using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Orchestrates waves using the Model’s config/state, creates enemies via IEnemyFactory,
    /// and binds return-to-pool subscriptions. No Unity fields here.
    /// </summary>
    public sealed class EnemyWaveSpawnerPresenter : IInitializable, IDisposable
    {
        private readonly EnemyWaveSpawnerView m_view;
        private readonly IEnemyWaveSpawnerModel m_model;
        private readonly IEnemyFactory m_enemyFactory;

        private readonly CompositeDisposable m_disposables = new CompositeDisposable();
        private CancellationTokenSource m_cts;
        private System.Random m_rng;

        [Inject]
        public EnemyWaveSpawnerPresenter(
            EnemyWaveSpawnerView view,
            IEnemyWaveSpawnerModel model,
            IEnemyFactory enemyFactory)
        {
            m_view = view;
            m_model = model;
            m_enemyFactory = enemyFactory;
        }

        public void Initialize()
        {
            m_rng = (m_model.RandomSeed == 0) ? new System.Random() : new System.Random(m_model.RandomSeed);
            m_cts = new CancellationTokenSource();

            // may expose WaveIndex to a HUD via m_model.WaveIndex.

            RunLoop(m_cts.Token).Forget();
        }

        public void Dispose()
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_disposables.Dispose();
        }

        private async UniTaskVoid RunLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SpawnSingleWave(token);

                m_model.AdvanceWave();
                await UniTask.Delay(m_model.WaveInterval, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        private void SpawnSingleWave(CancellationToken token)
        {
            var wave = m_model.Wave;
            if (wave == null || wave.entries == null) return;

            for (int e = 0; e < wave.entries.Count && !token.IsCancellationRequested; e++)
            {
                var entry = wave.entries[e];
                if (entry == null || entry.stats == null || entry.countPerWave <= 0)
                    continue;

                for (int i = 0; i < entry.countPerWave && !token.IsCancellationRequested; i++)
                {
                    Vector3 pos = GetOffScreenPosition(m_view.Cam, m_model.OffscreenPadding, m_rng);

                    IEnemyHandle handle = null;
                    try
                    {
                        handle = m_enemyFactory.Create(entry.stats, pos);
                    }
                    catch (InvalidOperationException)
                    {
                        // Pool currently full; skip gracefully.
                        continue;
                    }

                    handle.ReturnedToPool
                          .Take(1)
                          .Subscribe(_ => handle.Release())
                          .AddTo(handle.View);

                    handle.Spawn();
                }
            }
        }

        private static Vector3 GetOffScreenPosition(Camera camera, float padding, System.Random rng)
        {
            camera = camera != null ? camera : Camera.main;
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
