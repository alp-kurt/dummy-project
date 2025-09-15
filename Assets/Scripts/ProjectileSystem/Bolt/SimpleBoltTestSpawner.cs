using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class SimpleBoltTestSpawner : MonoBehaviour
    {
        [SerializeField] private BoltConfig boltConfig;
        [SerializeField] private Vector3 spawnDir = Vector3.right;

        private IBoltFactory _factory;

        [Inject] public void Construct(IBoltFactory factory) => _factory = factory;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                var pos = transform.position;
                var handle = _factory.Create(pos, spawnDir, boltConfig);
                Debug.Log($"Spawned Bolt: {boltConfig.DisplayName} dmg={boltConfig.BaseDamage} speed={boltConfig.BaseSpeed}");
            }
        }
    }
}
