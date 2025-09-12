using Zenject;

namespace Scripts
{
    public sealed class EnemyDeathStreamInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyDeathStream>().AsSingle().NonLazy();
        }
    }
}
