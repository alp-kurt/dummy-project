using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Drives enemy wave spawning on an async loop:
    /// - Loops waves with a fixed interval.
    /// - Spawns each wave with per-frame budget or per-spawn delay pacing.
    /// - Creates enemies via factory and wires pool return -> release.
    /// Off-screen spawn positions are randomized around the camera edges.
    /// </summary>
    public sealed class EnemyWaveSpawnerPresenter : IInitializable, IDisposable
    {
        private readonly EnemyWaveSpawnerView _view;
        private readonly IEnemyWaveSpawnerModel _model;
        private readonly IEnemyFactory _enemyFactory;
        private readonly Camera _camera;

        /// <summary>
        /// Optional optimization interface: factory can avoid throwing on unavailable prefabs
        /// and simply return false when creation isn't possible.
        /// </summary>
        public interface IEnemyFactoryTry
        {
            bool TryCreate(EnemyStats stats, Vector3 worldPosition, out IEnemyHandle handle);
        }

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private CancellationTokenSource _cts;
        private System.Random _rng;

        /// <summary>
        /// DI entry point. The <paramref name="camera"/> comes from your Player/Scene installer.
        /// </summary>
        [Inject]
        public EnemyWaveSpawnerPresenter(
            EnemyWaveSpawnerView view,
            IEnemyWaveSpawnerModel model,
            IEnemyFactory enemyFactory,
            Camera camera) // <- injected from PlayerInstaller
        {
            _view = view;
            _model = model;
            _enemyFactory = enemyFactory;
            _camera = camera;
        }

        /// <summary>
        /// Initializes RNG and starts the async wave loop.
        /// </summary>
        public void Initialize()
        {
            _rng = (_model.RandomSeed == 0) ? new System.Random() : new System.Random(_model.RandomSeed);
            _cts = new CancellationTokenSource();
            RunLoopAsync(_cts.Token).Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _disposables.Dispose();
        }

        /// <summary>
        /// Main async driver:
        /// - Spawn a single wave
        /// - Advance wave index/state in the model
        /// - Wait for the configured interval and repeat
        /// </summary>
        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await SpawnSingleWaveAsync(token);
                _model.AdvanceWave();
                await UniTask.Delay(_model.WaveInterval, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>
        /// Emits the current wave's entries with pacing:
        /// - If SpawnDelaySeconds &gt; 0 -> wait that delay per spawn.
        /// - Else if SpawnBudgetPerFrame &gt; 0 -> yield after N spawns in the same frame.
        /// - Else -> yield each spawn to avoid long frames.
        ///
        /// Each created enemy:
        /// - Subscribes to ReturnedToPool once -> Release handle.
        /// - Then Spawn() is called to begin its lifetime.
        /// </summary>
        private async UniTask SpawnSingleWaveAsync(CancellationToken token)
        {
            var wave = _model.Wave;
            if (wave == null || wave.Entries == null) return;

            int budget = Mathf.Max(0, _view.SpawnBudgetPerFrame);
            float delaySec = Mathf.Max(0f, _view.SpawnDelaySeconds);
            bool useDelay = delaySec > 0f;
            int emittedThisFrame = 0;

            for (int e = 0; e < wave.Entries.Count && !token.IsCancellationRequested; e++)
            {
                var entry = wave.Entries[e];
                if (entry == null || entry.Stats == null || entry.CountPerWave <= 0)
                    continue;

                for (int i = 0; i < entry.CountPerWave && !token.IsCancellationRequested; i++)
                {
                    Vector3 pos = GetOffScreenPosition(_camera, _model.OffscreenPadding, _rng);

                    IEnemyHandle handle = null;
                    if (_enemyFactory is IEnemyFactoryTry tryFactory)
                    {
                        // Preferred: non-throwing attempt (pool empty, etc.)
                        if (!tryFactory.TryCreate(entry.Stats, pos, out handle))
                            continue;
                    }
                    else
                    {
                        // Fallback: may throw if creation is not possible; swallow and skip
                        try
                        {
                            handle = _enemyFactory.Create(entry.Stats, pos);
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }
                    }

                    // Auto-release handle when the enemy returns to pool (one-shot).
                    handle.ReturnedToPool
                          .Take(1)
                          .Subscribe(_ => handle.Release())
                          .AddTo(handle.View); // tie subscription to the enemy view lifetime

                    handle.Spawn();

                    if (useDelay)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(delaySec), DelayType.DeltaTime, PlayerLoopTiming.Update, token);
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
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a random position just outside the camera's view rectangle:
        /// picks one of four sides uniformly, then samples uniformly along that side,
        /// offsetting outward by <paramref name="padding"/>.
        /// </summary>
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
            switch (side)
            {
                case 0: return new Vector3(left - padding, Mathf.Lerp(bottom, top, (float)rng.NextDouble()), 0f);
                case 1: return new Vector3(right + padding, Mathf.Lerp(bottom, top, (float)rng.NextDouble()), 0f);
                case 2: return new Vector3(Mathf.Lerp(left, right, (float)rng.NextDouble()), bottom - padding, 0f);
                default: return new Vector3(Mathf.Lerp(left, right, (float)rng.NextDouble()), top + padding, 0f);
            }
        }
    }
}
