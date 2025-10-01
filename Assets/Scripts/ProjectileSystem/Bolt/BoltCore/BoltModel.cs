using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class BoltModel : ProjectileModel, IBoltModel
    {
        private float _lifetimeSeconds;
        private readonly FloatReactiveProperty _remainingLifetime = new();

        public float LifetimeSeconds => _lifetimeSeconds;
        public IReadOnlyReactiveProperty<float> RemainingLifetimeRx => _remainingLifetime;
        public bool IsExpired => _lifetimeSeconds > 0f && _remainingLifetime.Value <= 0f;

        public BoltModel(string name, Sprite sprite, int damage, float speed, float lifetimeSeconds = 6f)
            : base(name, sprite, damage, speed)
        {
            _lifetimeSeconds = Mathf.Max(0f, lifetimeSeconds);
            _remainingLifetime.Value = _lifetimeSeconds;
        }

        public void ResetLifetime(float? newLifetimeSeconds = null)
        {
            if (newLifetimeSeconds.HasValue)
                _lifetimeSeconds = Mathf.Max(0f, newLifetimeSeconds.Value);
            _remainingLifetime.Value = _lifetimeSeconds;
        }

        public void TickLifetime(float deltaTime)
        {
            if (!IsActive || _lifetimeSeconds <= 0f || deltaTime <= 0f) return;
            var next = _remainingLifetime.Value - deltaTime;
            _remainingLifetime.Value = next <= 0f ? 0f : next;
        }

        protected override void OnActivated()
        {
            _remainingLifetime.Value = _lifetimeSeconds;
        }
    }
}
