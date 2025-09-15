namespace Scripts
{
    /// <summary>
    /// Creates the full glue (Model + Presenter) for a given EnemyView and EnemyStats.
    /// </summary>
    public interface IEnemyPresenterFactory
    {
        EnemyPresenter Create(EnemyView view, EnemyStats stats);
    }
}
