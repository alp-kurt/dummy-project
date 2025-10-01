using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BoltSpawnerConfig", menuName = "Game/Projectiles/Bolt Spawner Config")]
    public sealed class BoltSpawnerConfig : ScriptableObject
    {
        [Header("Casting")]
        [Tooltip("How many bolts to spawn each time a cast triggers.")]
        [Min(1)] public int boltsPerCast = 1;

        [Tooltip("Seconds between casts. Presenter will accumulate Time.deltaTime and trigger casts accordingly.")]
        [Min(0.05f)] public float secondsBetweenCasts = 0.5f;

        [Header("Debug")]
        [Tooltip("Optional: Log each cast to the console.")]
        public bool logCasts = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (boltsPerCast < 1) boltsPerCast = 1;
            if (secondsBetweenCasts < 0.05f) secondsBetweenCasts = 0.05f;
        }
#endif
    }
}
