using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Designers can duplicate these SOs per level (e.g., Minion_Lv1, Minion_Lv5, Champion_Lv10) and tweak health/speed/damage/sprite/name for difficulty curves.
    /// </summary>
    public abstract class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Enemy";
        public EnemyArchetype Archetype;

        [Header("Visual")]
        public Sprite Sprite;

        [Header("Stats")]
        [Min(1)] public int MaxHealth = 5;
        [Min(0f)] public float MoveSpeed = 1f;
        [Min(0)] public int Damage = 1;
    }
}
