using UnityEngine;

namespace Scripts
{
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
        }
    }
}
