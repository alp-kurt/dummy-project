using UniRx;

namespace Scripts
{
    public interface IHasLifetime
    {
        float LifetimeSeconds { get; }
        IReadOnlyReactiveProperty<float> RemainingLifetimeRx { get; }
        bool IsExpired { get; }

        /// <summary>Reset remaining lifetime to current LifetimeSeconds or to a new value.</summary>
        void ResetLifetime(float? newLifetimeSeconds = null);

        /// <summary>Advance lifetime by deltaTime seconds.</summary>
        void TickLifetime(float deltaTime);
    }
}
