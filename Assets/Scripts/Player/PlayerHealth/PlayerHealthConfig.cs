using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Game/Configs/Player Health Config")]
    public sealed class PlayerHealthConfig : ScriptableObject
    {
        [Min(1f)] public float MaxHealth = 20f;
    }
}