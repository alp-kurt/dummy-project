using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyWaveSpawnerPresenter : IInitializable, IDisposable
    {
        private readonly EnemyWaveSpawnerView _view;
        private readonly IEnemyWaveSpawnerModel _model;
        private readonly IEnemyFactory _enemyFactory;
        private readonly Camera _camera;

        public interface IEnemyFactoryTry
        {
            bool TryCreate(EnemyStats stats, Vector3 worldPosition, out IEnemyHandle handle);
        }

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private CancellationTokenSource _cts;
        private System.Random _rng;

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

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await SpawnSingleWaveAsync(token);
                _model.AdvanceWave();
                await UniTask.Delay(_model.WaveInterval, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

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
                        if (!tryFactory.TryCreate(entry.Stats, pos, out handle))
                            continue;
                    }
                    else
                    {
                        try
                        {
                            handle = _enemyFactory.Create(entry.Stats, pos);
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }
                    }

                    handle.ReturnedToPool
                          .Take(1)
                          .Subscribe(_ => handle.Release())
                          .AddTo(handle.View);

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
