using UniRx;
using UnityEngine;

namespace Scripts
{
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
                    .Subscribe(__ =>
                    {
                        TickRicochet();
                        TickLifetimeAndMaybeDespawn();
                    })
            );
        }

        private void TickLifetimeAndMaybeDespawn()
        {
            if (_model is IBoltModel life)
            {
                life.TickLifetime(Time.deltaTime);
                if (life.IsExpired) RequestDespawn();
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
                    Mathf.Clamp(vp.x, 0.001f, 0.999f),
                    Mathf.Clamp(vp.y, 0.001f, 0.999f),
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
                SetDirection(dir.sqrMagnitude > 0f ? dir.normalized : SafeRandomDirection());
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
            EnemyView closestView = null;
            float bestSq = float.PositiveInfinity;

            var views = _activeEnemiesRoot.GetComponentsInChildren<EnemyView>(includeInactive: false);
            foreach (var v in views)
            {
                if (!v || !v.gameObject.activeInHierarchy) continue;
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
