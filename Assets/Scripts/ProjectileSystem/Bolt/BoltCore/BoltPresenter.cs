using UniRx;
using UnityEngine;

namespace Scripts
{
    public class BoltPresenter : ProjectilePresenter
    {
        private readonly IBoltTargetingService _targetingService;
        private readonly Camera _camera;

        private readonly float _edgePaddingWorld;
        private readonly float _ricochetCooldown;
        private float _ricochetCdTimer;

        public BoltPresenter(
            IBoltModel model,
            BoltView view,
            Camera camera,
            IBoltTargetingService targetingService,
            float edgePaddingWorld = 0.25f,
            float ricochetCooldown = 0.08f
        ) : base(model, view)
        {
            _camera = camera;
            _targetingService = targetingService;
            _edgePaddingWorld = Mathf.Max(0f, edgePaddingWorld);
            _ricochetCooldown = Mathf.Max(0f, ricochetCooldown);
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

            UpdateRicochetCooldown();

            var position = GetPosition();
            var viewportPoint = _camera.WorldToViewportPoint(position);
            var viewportPadding = GetViewportPadding(position);

            if (!IsOutsideViewport(viewportPoint, viewportPadding) || !IsRicochetReady())
                return;

            SetDirection(_targetingService.GetRedirectDirection(GetPosition()));
            _ricochetCdTimer = _ricochetCooldown;

            var clampedViewportPoint = ClampViewportPoint(viewportPoint, viewportPadding);
            var clampedPosition = _camera.ViewportToWorldPoint(clampedViewportPoint);
            View.SetPosition(clampedPosition);
        }

        private void UpdateRicochetCooldown()
        {
            if (_ricochetCdTimer <= 0f) return;
            _ricochetCdTimer -= Time.deltaTime;
        }

        private bool IsRicochetReady() => _ricochetCdTimer <= 0f;

        private Vector2 GetViewportPadding(Vector3 worldPosition)
        {
            if (_edgePaddingWorld <= 0f) return Vector2.zero;

            var rightOffset = _camera.WorldToViewportPoint(worldPosition + Vector3.right * _edgePaddingWorld);
            var upOffset = _camera.WorldToViewportPoint(worldPosition + Vector3.up * _edgePaddingWorld);
            var origin = _camera.WorldToViewportPoint(worldPosition);

            return new Vector2(
                Mathf.Abs(rightOffset.x - origin.x),
                Mathf.Abs(upOffset.y - origin.y)
            );
        }

        private static bool IsOutsideViewport(Vector3 viewportPoint, Vector2 padding)
        {
            var safePadding = ClampViewportPadding(padding);
            bool outsideX = (viewportPoint.x < safePadding.x) || (viewportPoint.x > 1f - safePadding.x);
            bool outsideY = (viewportPoint.y < safePadding.y) || (viewportPoint.y > 1f - safePadding.y);
            return outsideX || outsideY;
        }

        private static Vector3 ClampViewportPoint(Vector3 viewportPoint, Vector2 padding)
        {
            var safePadding = ClampViewportPadding(padding);
            return new Vector3(
                Mathf.Clamp(viewportPoint.x, safePadding.x, 1f - safePadding.x),
                Mathf.Clamp(viewportPoint.y, safePadding.y, 1f - safePadding.y),
                viewportPoint.z
            );
        }

        private static Vector2 ClampViewportPadding(Vector2 padding)
        {
            return new Vector2(
                Mathf.Clamp(padding.x, 0f, 0.49f),
                Mathf.Clamp(padding.y, 0f, 0.49f)
            );
        }

    }
}
