using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class BoltModel : ProjectileModel, IBoltModel, IHasLifetime
    {
        private float m_lifetimeSeconds;
        private readonly FloatReactiveProperty m_remainingLifetime = new();

        public float LifetimeSeconds => m_lifetimeSeconds;
        public IReadOnlyReactiveProperty<float> RemainingLifetimeRx => m_remainingLifetime;
        public bool IsExpired => m_lifetimeSeconds > 0f && m_remainingLifetime.Value <= 0f;

        public BoltModel(string name, Sprite sprite, ProjectileDamage damage, ProjectileSpeed speed, float lifetimeSeconds = 6f)
            : base(name, sprite, damage, speed)
        {
            m_lifetimeSeconds = Mathf.Max(0f, lifetimeSeconds);
            m_remainingLifetime.Value = m_lifetimeSeconds;
        }

        public void ResetLifetime(float? newLifetimeSeconds = null)
        {
            if (newLifetimeSeconds.HasValue)
                m_lifetimeSeconds = Mathf.Max(0f, newLifetimeSeconds.Value);

            m_remainingLifetime.Value = m_lifetimeSeconds;
        }

        public void TickLifetime(float deltaTime)
        {
            if (!IsActive || m_lifetimeSeconds <= 0f || deltaTime <= 0f) return;
            var next = m_remainingLifetime.Value - deltaTime;
            m_remainingLifetime.Value = next <= 0f ? 0f : next;
        }

        protected override void OnActivated()
        {
            m_remainingLifetime.Value = m_lifetimeSeconds;
        }
    }
}
