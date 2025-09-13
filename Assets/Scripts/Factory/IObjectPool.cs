namespace Scripts
{
    public interface IObjectPool<T>
    {
        T GetObject();
        bool TryGetObject(out T instance);
        void ReleaseObject(T obj);

        int AvailableCount { get; }
        int InUseCount { get; }
    }
}
