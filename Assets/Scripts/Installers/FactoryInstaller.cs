using Zenject;

namespace Scripts
{
    public sealed class FactoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameFactory>().AsSingle();
        }
    }
}
