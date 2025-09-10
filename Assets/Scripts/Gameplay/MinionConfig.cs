using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "MinionConfig", menuName = "Configs/Enemies/Minion")]
    public class MinionConfig : EnemyConfig
    {
        private void OnEnable()
        {
            Archetype = EnemyArchetype.Minion;
        }
    }
}
