using UniRx;

namespace Scripts
{
    /// <summary>
    /// Semantic alias for a counter specifically tracking enemy kills.
    /// Keeps the same API as ICounter to avoid presenter/view changes.
    /// </summary>
    public interface IEnemyKillCounterModel : ICounter
    {
        // No extra members for now—semantic meaning only.
    }
}
