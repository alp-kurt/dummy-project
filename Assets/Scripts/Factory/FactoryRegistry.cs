using System;
using System.Collections.Generic;

namespace Scripts
{
    public sealed class FactoryRegistry : IFactoryRegistry
    {
        private readonly Dictionary<Type, object> m_map = new Dictionary<Type, object>();

        public void Register<T>(IObjectFactory<T> factory)
        {
            m_map[typeof(T)] = factory;
        }

        public bool TryGet<T>(out IObjectFactory<T> factory)
        {
            if (m_map.TryGetValue(typeof(T), out var obj) && obj is IObjectFactory<T> typed)
            {
                factory = typed;
                return true;
            }
            factory = default;
            return false;
        }
    }
}
