using System;

namespace Scripts
{
    public sealed class AbstractFactory : IAbstractFactory
    {
        private readonly IFactoryRegistry m_registry;

        public AbstractFactory(IFactoryRegistry registry)
        {
            m_registry = registry;
        }

        public T Create<T>()
        {
            if (!m_registry.TryGet<T>(out var factory))
                throw new InvalidOperationException($"No factory registered for type {typeof(T).Name}");
            return factory.CreateNew();
        }
    }
}
