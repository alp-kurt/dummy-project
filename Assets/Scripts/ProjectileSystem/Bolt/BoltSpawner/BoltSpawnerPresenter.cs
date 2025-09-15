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

            for (int i = 0; i < count; i++)
            {
                Vector2 rand = Random.insideUnitCircle;
                if (rand.sqrMagnitude < 1e-6f) rand = Vector2.right; else rand.Normalize();
                Vector3 dir = new Vector3(rand.x, rand.y, 0f);

                _factory.Create(origin, dir, _boltCfg);
            }

            if (_cfg.logCasts)
                Debug.Log($"[BoltSpawner] Cast {count} bolt(s) at t={Time.time:0.00}s, interval={_model.IntervalSeconds:0.00}");
        }

        public void Dispose() { }
    }
}
