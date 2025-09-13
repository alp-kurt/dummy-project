using UnityEngine;

namespace Scripts
{
    // Base MonoBehaviour for any pooled View
    public abstract class PooledView : MonoBehaviour, IPoolableObject
    {
        public GameObject GameObject => gameObject;

        public virtual void OnRent()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
            transform.SetParent(null, false);
        }
    }
}
