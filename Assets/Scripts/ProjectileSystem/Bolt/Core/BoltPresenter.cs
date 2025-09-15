using UniRx;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Bolt specialisation: on screen-edge, redirect toward closest active enemy (fallback random);
    /// also ticks lifetime and despawns when expired.
    /// </summary>
    public class BoltPresenter : ProjectilePresenter
    {
        private readonly Transform _activeEnemiesRoot;
        private readonly Camera _camera;

        private readonly float _edgePaddingWorld;
        private readonly float _ricochetCooldown;

        private float _ricochetCdTimer;

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

            AttachToSpawn(
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        TickRicochet();
                        TickLifetimeAndMaybeDespawn();
                    })
            );
        }

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

        private void TickRicochet()
        {
            if (_camera == null) return;

            if (_ricochetCdTimer > 0f)
                _ricochetCdTimer -= Time.deltaTime;

            var pos = GetPosition();
            var vp = _camera.WorldToViewportPoint(pos);

            bool outsideX = (vp.x < 0f) || (vp.x > 1f);
            bool outsideY = (vp.y < 0f) || (vp.y > 1f);

            if ((outsideX || outsideY) && _ricochetCdTimer <= 0f)
            {
                RedirectTowardClosestEnemyOrRandom();
                _ricochetCdTimer = _ricochetCooldown;

                var clampedVp = new Vector3(
                    Mathf.Clamp(vp.x, 0.0f + 0.001f, 1.0f - 0.001f),
                    Mathf.Clamp(vp.y, 0.0f + 0.001f, 1.0f - 0.001f),
                    vp.z
                );
                var clampedPos = _camera.ViewportToWorldPoint(clampedVp);
                View.SetPosition(clampedPos);
            }
        }

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

        private Vector3 SafeRandomDirection()
        {
            var rnd = UnityEngine.Random.insideUnitCircle;
            if (rnd == Vector2.zero) rnd = Vector2.right;
            rnd.Normalize();
            return new Vector3(rnd.x, rnd.y, 0f);
        }

        private Transform FindClosestActiveEnemy()
        {
            if (_activeEnemiesRoot == null) return null;

            var pos = GetPosition();
            Transform closest = null;
            float bestSq = float.PositiveInfinity;

            for (int i = 0, c = _activeEnemiesRoot.childCount; i < c; i++)
            {
                var child = _activeEnemiesRoot.GetChild(i);
                if (!child.gameObject.activeInHierarchy) continue;

                // Optional: ensure it's a valid target (component/tag/layer)
                var sq = (child.position - pos).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; closest = child; }
            }
            return closest;
        }
    }
}
