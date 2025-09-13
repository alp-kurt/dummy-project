namespace Scripts
{
    public interface IObjectFactory<T>
    {
        T CreateNew();
    }
}
