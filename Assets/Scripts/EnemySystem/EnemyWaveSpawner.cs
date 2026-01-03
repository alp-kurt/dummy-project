using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class EnemyWaveSpawner : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Config (required)")]
        [SerializeField] private EnemyWaveConfig _wave;

        [Header("Pacing (optional)")]
        [Tooltip("0 = ignore (spawn all instantly or per-frame).")]
        [SerializeField, Min(0)] private int _spawnBudgetPerFrame = 8;

        [Tooltip("Delay between consecutive spawns inside a wave (seconds). 0 = no delay.")]
        [SerializeField, Min(0f)] private float _spawnDelaySeconds = 0f;

        [Header("Randomness")]
        [Tooltip("0 = non-deterministic; non-zero = deterministic spawn positions.")]
        [SerializeField] private int _randomSeed = 0;

        private readonly CompositeDisposable _cd = new();
        private IEnemyFactory _enemyFactory;
        private Camera _camera;
        private System.Random _rng;

        [Inject]
        private void Construct(IEnemyFactory enemyFactory, [Inject(Optional = true)] Camera camera)
        {
            _enemyFactory = enemyFactory;
            _camera = camera;
        }

        public void Initialize()
        {
            if (_wave == null)
            {
                Debug.LogWarning("[EnemyWaveSpawner] WaveConfig not assigned, disabling.", this);
                enabled = false;
                return;
            }

            _rng = (_randomSeed == 0) ? new System.Random() : new System.Random(_randomSeed);

            var interval = TimeSpan.FromSeconds(Mathf.Max(0.05f, _wave.WaveIntervalSeconds));
            Observable.Interval(interval)
                .StartWith(0L)
                .SelectMany(_ => SpawnSingleWave())
                .Subscribe()
                .AddTo(_cd);
        }

        public void Dispose() => _cd.Dispose();

        private IObservable<Unit> SpawnSingleWave()
        {
            if (_wave.Entries == null || _wave.Entries.Count == 0) return Observable.Empty<Unit>();

            var padding = Mathf.Max(0f, _wave.OffscreenPadding);
            var budget = Mathf.Max(0, _spawnBudgetPerFrame);
            var delay = Mathf.Max(0f, _spawnDelaySeconds);

            IObservable<Unit> seq = Observable.Empty<Unit>();

            foreach (var entry in _wave.Entries)
            {
                if (entry == null || entry.Stats == null || entry.CountPerWave <= 0) continue;
                seq = seq.Concat(BuildEntrySequence(entry, padding, budget, delay));
            }

            return seq;
        }

        private IObservable<Unit> BuildEntrySequence(EnemyWaveConfig.Entry entry, float padding, int budget, float delaySec)
        {
            int count = entry.CountPerWave;

            // Case 1: explicit per-spawn delay inside the wave
            if (delaySec > 0f)
            {
                return Observable.Range(0, count)
                    .Select(i =>
                        Observable.Timer(TimeSpan.FromSeconds(i * delaySec))
                            .Do(__ => TrySpawnOne(entry.Stats, padding))
                            .AsUnitObservable())
                    .Merge();
            }

            // Case 2: budget per frame
            if (budget > 0)
            {
                return Observable.Range(0, count)
                    .Buffer(budget)
                    .Select(batch =>
                        Observable.EveryUpdate()
                            .Take(1)
                            .Do(__ =>
                            {
                                for (int i = 0; i < batch.Count; i++)
                                    TrySpawnOne(entry.Stats, padding);
                            })
                            .AsUnitObservable())
                    .Concat();
            }

            // Case 3: no pacing — still schedule on frames to avoid long stalls
            return Observable.Range(0, count)
                .Select(_ =>
                    Observable.EveryUpdate()
                        .Take(1)
                        .Do(__ => TrySpawnOne(entry.Stats, padding))
                        .AsUnitObservable())
                .Concat();
        }

        private void TrySpawnOne(EnemyStats stats, float padding)
        {
            var cam = _camera ? _camera : Camera.main;
            if (!cam) return;

            Vector3 pos = GetOffScreenPosition(cam, padding, _rng);

            IEnemyHandle handle;
            try { handle = _enemyFactory.Create(stats, pos); }
            catch { return; } // pool exhausted or factory not ready

            handle.Spawn();
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

            float RandomRange(float min, float max)
            {
                return min + (max - min) * (float)rng.NextDouble();
            }
            switch (side)
            {
                case 0: return new Vector3(left - padding, RandomRange(bottom, top), 0f);
                case 1: return new Vector3(right + padding, RandomRange(bottom, top), 0f);
                case 2: return new Vector3(RandomRange(left, right), bottom - padding, 0f);
                default: return new Vector3(RandomRange(left, right), top + padding, 0f);
            }
        }
    }
}
