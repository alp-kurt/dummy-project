using UniRx;

namespace Scripts
{
    /// <summary>
    /// Minimal reactive counter abstraction.
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// Reactive count stream (emits current value immediately and on every change).
        /// </summary>
        IReadOnlyReactiveProperty<int> Count { get; }

        /// <summary>Increment by 1.</summary>
        void Increment();

        /// <summary>Add an arbitrary delta (can be negative for decrement).</summary>
        void Add(int delta);

        /// <summary>Reset to 0.</summary>
        void Reset();
    }
}
