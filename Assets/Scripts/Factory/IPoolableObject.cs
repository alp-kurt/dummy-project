using UnityEngine;

namespace Scripts
{
    public interface IPoolableObject
    {
        // Called immediately after the object is rented from the pool.
        void OnRent();

        // Called right before the object is returned to the pool.
        void OnRelease();

        // Recommended: turn the object on/off, reset transient state, etc.
        GameObject GameObject { get; }


    }
}
