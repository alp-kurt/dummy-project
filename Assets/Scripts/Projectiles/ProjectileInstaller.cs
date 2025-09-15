using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class ProjectileInstaller : MonoInstaller
    {
        [SerializeField] private ProjectileView _prefabProjectile; // the single base prefab

        public override void InstallBindings()
        {
            // Pool the single base projectile prefab for Bolt usage


            // Per-type factory
        
        }
    }
}
