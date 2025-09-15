using UnityEngine;

namespace Scripts
{
    public abstract class EnemyStats : ScriptableObject
    {
        [Header("Gameplay")]
        [Min(1)] public int maxHealth = 3;
        [Min(0f)] public float movementSpeed = 2.5f;
        [Min(1)] public int damage = 1;

        [Header("Visuals")]
        public Sprite sprite;
        public float spriteScale = 1f;
    }
}
