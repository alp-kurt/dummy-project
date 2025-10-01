using UniRx;

namespace Scripts
{
    public interface IBoltModel : IProjectileModel
    {
        float LifetimeSeconds { get; }
        IReadOnlyReactiveProperty<float> RemainingLifetimeRx { get; }
        bool IsExpired { get; }

        void ResetLifetime(float? newLifetimeSeconds = null);
        void TickLifetime(float deltaTime);
    }
}