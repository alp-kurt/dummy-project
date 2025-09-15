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
    /// Spawns are paced (budget/delay) to avoid frame hitches on mobile.
    /// </summary>
    public sealed class EnemyWaveSpawnerPresenter : IInitializable, IDisposable
    {
        private readonly EnemyWaveSpawnerView m_view;
        private readonly IEnemyWaveSpawnerModel m_model;
        private readonly IEnemyFactory m_enemyFactory;

        // Optional extension interface: if implemented by the factory, avoid exception-based control flow.
        // If not implemented, we gracefully fall back to Create(...) + try/catch.
        public interface IEnemyFactoryTry
        {
            bool TryCreate(EnemyStats stats, Vector3 worldPosition, out IEnemyHandle handle);
        }

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

            RunLoopAsync(m_cts.Token).Forget();
        }

        public void Dispose()
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_disposables.Dispose();
        }

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await SpawnSingleWaveAsync(token);

                m_model.AdvanceWave();
                await UniTask.Delay(m_model.WaveInterval, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        private async UniTask SpawnSingleWaveAsync(CancellationToken token)
        {
            var wave = m_model.Wave;
            if (wave == null || wave.entries == null) return;

            // Choose pacing mode
            int budget = Mathf.Max(0, m_view.SpawnBudgetPerFrame);
            float delaySec = Mathf.Max(0f, m_view.SpawnDelaySeconds);
            bool useDelay = delaySec > 0f;
            int emittedThisFrame = 0;

            for (int e = 0; e < wave.entries.Count && !token.IsCancellationRequested; e++)
            {
                var entry = wave.entries[e];
                if (entry == null || entry.stats == null || entry.countPerWave <= 0)
                    continue;

                for (int i = 0; i < entry.countPerWave && !token.IsCancellationRequested; i++)
                {
                    Vector3 pos = GetOffScreenPosition(m_view.Cam, m_model.OffscreenPadding, m_rng);

                    // Prefer a non-throwing TryCreate if factory supports it
                    IEnemyHandle handle = null;
                    if (m_enemyFactory is IEnemyFactoryTry tryFactory)
                    {
                        if (!tryFactory.TryCreate(entry.stats, pos, out handle))
                        {
                            // Pool is probably full; skip this spawn quietly
                            continue;
                        }
                    }
                    else
                    {
                        try
                        {
                            handle = m_enemyFactory.Create(entry.stats, pos);
                        }
                        catch (InvalidOperationException)
                        {
                            // Pool full or not ready; skip gracefully
                            continue;
                        }
                    }

                    // Lifecycle: release handle on first ReturnedToPool
                    handle.ReturnedToPool
                          .Take(1)
                          .Subscribe(_ => handle.Release())
                          .AddTo(handle.View);

                    handle.Spawn();

                    // Pacing: either wait a fixed delay or yield per budget
                    if (useDelay)
                    {
                        await UniTask.Delay(
                            TimeSpan.FromSeconds(delaySec),
                            DelayType.DeltaTime,
                            PlayerLoopTiming.Update,
                            token
                        );
                    }
                    else if (budget > 0)
                    {
                        emittedThisFrame++;
                        if (emittedThisFrame >= budget)
                        {
                            emittedThisFrame = 0;
                            await UniTask.Yield(PlayerLoopTiming.Update);
                        }
                    }
                    else
                    {
                        // Default: yield next frame per spawn to be extra gentle
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
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

            // Choose a side: 0=left, 1=right, 2=bottom, 3=top
            int side = rng.Next(0, 4);
            switch (side)
            {
                case 0: // left
                    return new Vector3(left - padding, Mathf.Lerp(bottom, top, (float)rng.NextDouble()), 0f);
                case 1: // right
                    return new Vector3(right + padding, Mathf.Lerp(bottom, top, (float)rng.NextDouble()), 0f);
                case 2: // bottom
                    return new Vector3(Mathf.Lerp(left, right, (float)rng.NextDouble()), bottom - padding, 0f);
                case 3: // top
                default:
                    return new Vector3(Mathf.Lerp(left, right, (float)rng.NextDouble()), top + padding, 0f);
            }
        }
    }
}
