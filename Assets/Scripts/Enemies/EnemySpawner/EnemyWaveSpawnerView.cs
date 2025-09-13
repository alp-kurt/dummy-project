using UnityEngine;

namespace Scripts
{
    public sealed class EnemyWaveSpawnerView : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private Camera cam;
        [SerializeField] private WaveConfig wave;

        [Header("Randomness")]
        [Tooltip("0 = non-deterministic; non-zero = deterministic spawn positions")]
        [SerializeField] private int randomSeed = 12345;

        public Camera Cam => cam != null ? cam : Camera.main;
        public WaveConfig Wave => wave;
        public int RandomSeed => randomSeed;
    }
}
