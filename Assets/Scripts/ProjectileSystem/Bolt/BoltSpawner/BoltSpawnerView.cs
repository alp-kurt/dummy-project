using UnityEngine;

namespace Scripts
{
    public sealed class BoltSpawnerView : MonoBehaviour
    {
        [Header("Required")]
        [Tooltip("Designer-tunable casting parameters")]
        [SerializeField] private BoltSpawnerConfig _config;

        public BoltSpawnerConfig Config => _config;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_config)
                Debug.LogWarning($"[{nameof(BoltSpawnerView)}] Config is not assigned.", this);
        }
#endif
    }
}
