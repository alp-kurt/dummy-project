using UnityEngine;
using Zenject;
using System;

using Random = UnityEngine.Random; // Alias to avoid System.Random ambiguity

namespace Scripts
{
    /// <summary>
    /// Reads model heartbeat, spawns N bolts per cast at the Player's position.
    /// Initial direction is random on the unit circle. Pure DI, no serialized dependencies.
    /// </summary>
    public sealed class BoltSpawnerPresenter : ITickable, IInitializable, IDisposable
    {
        private readonly IBoltSpawnerModel _model;
        private readonly IBoltFactory _factory;
        private readonly IPlayerPosition _playerPos;
        private readonly BoltSpawnerConfig _cfg; // includes boltsPerCast, secondsBetweenCasts, logCasts
        private readonly BoltConfig _boltCfg;    // existing projectile stats

        public BoltSpawnerPresenter(
            IBoltSpawnerModel model,
            IBoltFactory factory,
            IPlayerPosition playerPos,
            BoltSpawnerConfig cfg,
            BoltConfig boltCfg)
        {
            _model = model;
            _factory = factory;
            _playerPos = playerPos;
            _cfg = cfg;
            _boltCfg = boltCfg;
        }

        public void Initialize()
        {
            if (_cfg == null) throw new Exception($"{nameof(BoltSpawnerPresenter)}: Spawner Config is null.");
            if (_boltCfg == null) throw new Exception($"{nameof(BoltSpawnerPresenter)}: BoltConfig is null.");
        }

        public void Tick()
        {
            if (_model.Tick(Time.deltaTime))
            {
                Cast();
                _model.ConsumeTrigger();
            }
        }

        private void Cast()
        {
            Vector3 origin = _playerPos.Position;
            int count = _model.BoltsPerCast;

            const float kStepDeg = 15f; // fixed spacing between bolts

            // Random base direction ONCE per cast
            Vector2 baseDir = Random.insideUnitCircle;
            if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector2.right; else baseDir.Normalize();

            // Center the spread around the baseDir (e.g., for 3 bolts → -15°, 0°, +15°)
            float startDeg = -kStepDeg * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float offsetDeg = startDeg + i * kStepDeg;
                Vector2 dir = Rotate(baseDir, offsetDeg);
                _factory.Create(origin, dir, _boltCfg);
            }
        }

        // Helper
        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float cs = Mathf.Cos(r);
            float sn = Mathf.Sin(r);
            return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs).normalized;
        }

        public void Dispose() { }
    }
}
