using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "ChampionConfig", menuName = "Configs/Enemies/Champion")]
    public class ChampionConfig : EnemyConfig
    {
        private void OnEnable()
        {
            Archetype = EnemyArchetype.Champion;
        }
    }
}
