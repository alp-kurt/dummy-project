namespace Scripts
{
    public interface IBoltPresenterFactory
    {
        BoltPresenter Create(BoltView view, BoltConfig config);
    }
}