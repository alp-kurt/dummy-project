using UnityEngine;

namespace Scripts
{
    public interface IBoltFactory
    {
        IBoltHandle Create(Vector3 position, Vector3 directionNormalized, BoltConfig config);
    }
}