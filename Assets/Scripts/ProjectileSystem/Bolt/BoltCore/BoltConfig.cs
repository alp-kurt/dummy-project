using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BoltConfig", menuName = "Game/Projectiles/Bolt Config")]
    public sealed class BoltConfig : ScriptableObject
    {
        [Header("Stats")]
        public string DisplayName = "Bolt";
        public Sprite Sprite;
        [Min(0)] public int Damage = 1;
        [Min(0)] public float Speed = 12f;

        [Header("Lifetime")]
        [Min(0f)] public float LifetimeSeconds = 6f;

        [Header("Visuals (Optional)")]
        [Min(0f)] public float ScaleOverride = 0f;
    }
}
