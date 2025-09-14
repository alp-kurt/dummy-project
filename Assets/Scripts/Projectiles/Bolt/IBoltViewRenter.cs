using UnityEngine;

namespace Scripts
{
    public interface IBoltViewRenter
    {
        BoltView Rent(Vector3 worldPosition);
        void Return(BoltView view);
    }
}
