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

        [Header("Pacing (anti-hitch)")]
        [Tooltip("Max enemies to create per frame; set 0 to ignore and use Spawn Delay instead.")]
        [Min(0)] [SerializeField] private int spawnBudgetPerFrame = 8;

        [Tooltip("Delay between consecutive spawns (seconds). Set 0 to spawn per frame based on budget.")]
        [Min(0f)] [SerializeField] private float spawnDelaySeconds = 0f;

        public Camera Cam => cam != null ? cam : Camera.main;
        public WaveConfig Wave => wave;
        public int RandomSeed => randomSeed;

        public int SpawnBudgetPerFrame => spawnBudgetPerFrame;
        public float SpawnDelaySeconds => spawnDelaySeconds;
    }
}
