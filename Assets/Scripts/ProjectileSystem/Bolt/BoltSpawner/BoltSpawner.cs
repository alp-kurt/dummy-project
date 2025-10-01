using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    [DisallowMultipleComponent]
    public sealed class BoltSpawner : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Required")]
        [SerializeField] private BoltSpawnerConfig _spawnerConfig;

        [Header("Spawn Origin")]
        [SerializeField] private Transform _origin;

        [Header("Cone")]
        [SerializeField, Min(0f)] private float _stepDegrees = 15f;

        [Header("Visuals")]
        [SerializeField] private Vector3 _spawnScale = new Vector3(0.25f, 0.25f, 1f);

        private readonly CompositeDisposable _cd = new();

        private IBoltFactory _factory;
        private PlayerView _player;

        [Inject] private BoltConfig _boltConfig;

        [Inject]
        void Construct(IBoltFactory factory, [Inject(Optional = true)] PlayerView player)
        {
            _factory = factory;
            _player = player;
        }

        public void Initialize()
        {
            if (_spawnerConfig == null)
            {
                Debug.LogWarning("[BoltSpawner] SpawnerConfig missing, disabling.", this);
                enabled = false;
                return;
            }
            if (_boltConfig == null)
            {
                Debug.LogError("[BoltSpawner] BoltConfig is not bound/assigned.", this);
                enabled = false;
                return;
            }

            if (_origin == null && _player != null) _origin = _player.transform;

            var interval = Math.Max(0.05f, _spawnerConfig.secondsBetweenCasts);

            Observable.Interval(TimeSpan.FromSeconds(interval))
                      .StartWith(0L)
                      .Subscribe(_ => Cast())
                      .AddTo(_cd);
        }

        public void Dispose() => _cd.Dispose();

        private void Cast()
        {
            if (_origin == null)
            {
                if (_player != null) _origin = _player.transform;
                if (_origin == null) return;
            }

            int count = Math.Max(1, _spawnerConfig.boltsPerCast);

            Vector2 baseDir = UnityEngine.Random.insideUnitCircle;
            if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector2.right; else baseDir.Normalize();

            float startDeg = -_stepDegrees * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float offsetDeg = startDeg + i * _stepDegrees;
                Vector2 dir = Rotate(baseDir, offsetDeg);

                IBoltHandle handle;
                try
                {
                    handle = _factory.Create(_origin.position, dir, _boltConfig);
                }
                catch (Exception)
                {
                    continue;
                }

                // Force desired spawn scale
                handle.View.CachedTransform.localScale = _spawnScale;

                // Auto-release on return to pool
                handle.ReturnedToPool
                      .Take(1)
                      .Subscribe(__ => handle.Release())
                      .AddTo(handle.View);

                // Factory already spawned with (position, dir)
            }

            if (_spawnerConfig.logCasts)
                Debug.Log($"[BoltSpawner] Cast {count} bolt(s) at {_origin.position}", this);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float cs = Mathf.Cos(r);
            float sn = Mathf.Sin(r);
            return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs).normalized;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep Z at 1 unless you intentionally change it
            if (Mathf.Approximately(_spawnScale.z, 0f)) _spawnScale.z = 1f;
        }
#endif
    }
}
