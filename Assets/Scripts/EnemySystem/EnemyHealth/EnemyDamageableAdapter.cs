using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// MonoBehaviour on the Enemy prefab (ideally on the collider root).
    /// Now purely DI-driven: health is injected from the per-enemy child container.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyDamageableAdapter : MonoBehaviour, IDamageable
    {
        private IEnemyHealthModel m_health;

        // Called once per spawn by EnemyPresenterFactory
        public void Initialize(IEnemyHealthModel health)
        {
            m_health = health;
        }

        public void ReceiveDamage(int amount)
        {
            m_health?.ReceiveDamage(amount);
        }
    }
}