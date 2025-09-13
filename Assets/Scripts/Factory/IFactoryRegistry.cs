using System;

namespace Scripts
{
    public interface IFactoryRegistry
    {
        void Register<T>(IObjectFactory<T> factory);
        bool TryGet<T>(out IObjectFactory<T> factory);
    }
}
