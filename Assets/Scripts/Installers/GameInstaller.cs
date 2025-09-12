using Zenject;

namespace Scripts
{
    public sealed class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Bind global systems here (audio bus, save system, signal bus, etc.)
            // Player-specific bindings now live in PlayerInstaller.
        }
    }
}
