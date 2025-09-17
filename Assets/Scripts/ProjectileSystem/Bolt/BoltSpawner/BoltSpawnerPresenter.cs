using UnityEngine;
using Zenject;
using System;

using Random = UnityEngine.Random; // Alias to avoid System.Random ambiguity

namespace Scripts
{
    /// <summary>
    /// Reads model heartbeat and, on each trigger, spawns N bolts at the player's position.
    /// Initial directions form a symmetric cone with fixed 15° spacing, centered on a new
    /// random direction each cast (one-time orientation; not homing). Pure DI.
    /// </summary>
    public sealed class BoltSpawnerPresenter : ITickable, IInitializable, IDisposable
    {
        private readonly IBoltSpawnerModel _model;
        private readonly IBoltFactory _factory;
        private readonly BoltSpawnerConfig _cfg; // boltsPerCast, secondsBetweenCasts, logCasts, etc.
        private readonly BoltConfig _boltCfg;    // projectile stats

        /// <summary>
        /// DI entry point.
        /// </summary>
        /// <param name="model">Spawner timing/heartbeat (produces cast triggers).</param>
        /// <param name="factory">Creates bolts with position + direction + config.</param>
        /// <param name="playerPos">Provides the world position to spawn from.</param>
        /// <param name="cfg">Spawner config (interval/count/logging).</param>
        /// <param name="boltCfg">Projectile config passed per bolt.</param>
        public BoltSpawnerPresenter(
            IBoltSpawnerModel model,
            IBoltFactory factory,
            BoltSpawnerConfig cfg,
            BoltConfig boltCfg)
        {
            _model = model;
            _factory = factory;
            _cfg = cfg;
            _boltCfg = boltCfg;
        }

        /// <summary>
        /// Basic guardrails to ensure required configs are bound.
        /// </summary>
        public void Initialize()
        {
            if (_cfg == null) throw new Exception($"{nameof(BoltSpawnerPresenter)}: Spawner Config is null.");
            if (_boltCfg == null) throw new Exception($"{nameof(BoltSpawnerPresenter)}: BoltConfig is null.");
        }

        /// <summary>
        /// Advances the spawner's heartbeat. When it emits a trigger, perform a cast and
        /// consume the trigger immediately to avoid duplicate casts within the same frame.
        /// </summary>
        public void Tick()
        {
            if (_model.Tick(Time.deltaTime))
            {
                Cast();
                _model.ConsumeTrigger();
            }
        }

        /// <summary>
        /// Spawns <c>BoltsPerCast</c> bolts at the player's position. The cast's center
        /// direction is randomized once per cast; each bolt is offset by ±15° steps to
        /// create a symmetric cone: e.g., for 3 bolts → -15°, 0°, +15° around the center.
        /// </summary>
        private void Cast()
        {
            int count = _model.BoltsPerCast;

            const float kStepDeg = 15f; // fixed spacing between neighboring bolts

            // Random base direction ONCE per cast
            Vector2 baseDir = Random.insideUnitCircle;
            if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector2.right; else baseDir.Normalize();

            // Center the spread around the baseDir (e.g., 3 bolts → -15°, 0°, +15°)
            float startDeg = -kStepDeg * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float offsetDeg = startDeg + i * kStepDeg;
                Vector2 dir = Rotate(baseDir, offsetDeg);
            }
        }

        /// <summary>
        /// Returns a normalized copy of vector <paramref name="v"/> rotated by <paramref name="degrees"/>.
        /// </summary>
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
