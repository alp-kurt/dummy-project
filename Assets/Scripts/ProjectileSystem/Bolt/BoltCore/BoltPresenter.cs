using UniRx;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Bolt specialisation: on screen-edge, redirect toward an active enemy (fallback random);
    /// also ticks lifetime and despawns when expired.
    /// </summary>
    public class BoltPresenter : ProjectilePresenter
    {
        private readonly Transform _activeEnemiesRoot;
        private readonly Camera _camera;

        private readonly float _edgePaddingWorld;
        private readonly float _ricochetCooldown;

        private float _ricochetCdTimer;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="model">Bolt model (stats/state)</param>
        /// <param name="view">Bolt view (transform/physics)</param>
        /// <param name="camera">Camera used for viewport-edge checks</param>
        /// <param name="activeEnemiesRoot">Root transform that contains active EnemyView instances</param>
        /// <param name="edgePaddingWorld">World-space padding applied after a ricochet to keep bolt just inside the screen</param>
        /// <param name="ricochetCooldown">Minimum seconds between edge retargets (prevents rapid re-triggers at the rim)</param>
        public BoltPresenter(
            IBoltModel model,
            BoltView view,
            Camera camera,
            Transform activeEnemiesRoot,
            float edgePaddingWorld = 0.25f,
            float ricochetCooldown = 0.08f
        ) : base(model, view)
        {
            _camera = camera;
            _activeEnemiesRoot = activeEnemiesRoot;
            _edgePaddingWorld = Mathf.Max(0f, edgePaddingWorld);
            _ricochetCooldown = Mathf.Max(0f, ricochetCooldown);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void OnSpawned()
        {
            _ricochetCdTimer = 0f;

            // Drive bolt behaviour per-frame for lifetime + edge ricochet.
            AttachToSpawn(
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        TickRicochet();
                        TickLifetimeAndMaybeDespawn();
                    })
            );
        }

        /// <summary>
        /// Ticks lifetime if the bolt implements IHasLifetime, and requests despawn when expired.
        /// </summary>
        private void TickLifetimeAndMaybeDespawn()
        {
            if (_model is IHasLifetime life)
            {
                life.TickLifetime(Time.deltaTime);
                if (life.IsExpired)
                {
                    // Signal the handle/factory to return this to the pool.
                    RequestDespawn();
                }
            }
        }

        /// <summary>
        /// Detects when the bolt crosses the viewport bounds; if so (and off cooldown),
        /// snap its direction toward the closest on-screen enemy (fallback: random),
        /// then clamp the bolt position just inside the viewport to avoid immediate re-trigger.
        /// </summary>
        private void TickRicochet()
        {
            if (_camera == null) return;

            if (_ricochetCdTimer > 0f)
                _ricochetCdTimer -= Time.deltaTime;

            var pos = GetPosition();
            var vp = _camera.WorldToViewportPoint(pos);

            // Outside on either axis counts as an edge hit.
            bool outsideX = (vp.x < 0f) || (vp.x > 1f);
            bool outsideY = (vp.y < 0f) || (vp.y > 1f);

            if ((outsideX || outsideY) && _ricochetCdTimer <= 0f)
            {
                RedirectTowardClosestEnemyOrRandom();
                _ricochetCdTimer = _ricochetCooldown;

                // Nudge the bolt back inside by a tiny viewport margin to prevent double-triggering.
                var clampedVp = new Vector3(
                    Mathf.Clamp(vp.x, 0.0f + 0.001f, 1.0f - 0.001f),
                    Mathf.Clamp(vp.y, 0.0f + 0.001f, 1.0f - 0.001f),
                    vp.z
                );

                // Convert back to world and apply.
                var clampedPos = _camera.ViewportToWorldPoint(clampedVp);
                View.SetPosition(clampedPos);
            }
        }

        /// <summary>
        /// Picks the closest visible enemy and snaps travel direction toward it;
        /// if none are available, chooses a safe random direction. (One-time impulse; not homing.)
        /// </summary>
        private void RedirectTowardClosestEnemyOrRandom()
        {
            var target = FindClosestActiveEnemy();
            if (target != null)
            {
                var dir = (target.position - GetPosition());
                if (dir.sqrMagnitude > 0f)
                    SetDirection(dir.normalized);
                else
                    SetDirection(SafeRandomDirection());
            }
            else
            {
                SetDirection(SafeRandomDirection());
            }
        }

        /// <summary>
        /// Returns a non-zero normalized random direction (guards against (0,0) from insideUnitCircle).
        /// </summary>
        private Vector3 SafeRandomDirection()
        {
            var rnd = UnityEngine.Random.insideUnitCircle;
            if (rnd == Vector2.zero) rnd = Vector2.right;
            rnd.Normalize();
            return new Vector3(rnd.x, rnd.y, 0f);
        }

        /// <summary>
        /// Scans under <see cref="_activeEnemiesRoot"/> for the nearest <see cref="EnemyView"/> that is
        /// active AND currently visible (i.e., not in the OutOfScreen state). Returns its transform or null.
        /// </summary>
        private Transform FindClosestActiveEnemy()
        {
            if (_activeEnemiesRoot == null) return null;

            var pos = GetPosition();
            EnemyView closestView = null;
            float bestSq = float.PositiveInfinity;

            // Only consider EnemyView components; cheap and precise
            var views = _activeEnemiesRoot.GetComponentsInChildren<EnemyView>(includeInactive: false);
            foreach (var v in views)
            {
                if (!v || !v.gameObject.activeInHierarchy) continue;

                // Only pick enemies whose state is NOT OutOfScreen (i.e., currently visible)
                if (!v.IsVisible) continue;

                float d2 = (v.Position - pos).sqrMagnitude;
                if (d2 < bestSq)
                {
                    bestSq = d2;
                    closestView = v;
                }
            }

            return closestView ? closestView.transform : null;
        }
    }
}
