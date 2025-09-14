namespace Scripts
{
    public sealed class BoltPresenter : ProjectilePresenter
    {
        public BoltPresenter(IBoltModel model, ProjectileView view)
            : base(model, view) { }
        // Override later for ricochet/pierce.
    }
}
