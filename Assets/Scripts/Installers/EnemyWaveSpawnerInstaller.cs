using Zenject;

namespace Scripts
{
    /// <summary>
    /// Binds the Spawner MVP parts. Assign 'view' in Inspector or it will
    /// find one via FromComponentInHierarchy at resolve time.
    /// </summary>
    public sealed class EnemyWaveSpawnerInstaller : MonoInstaller
    {
        public EnemyWaveSpawnerView view; // optional; can be left null

        public override void InstallBindings()
        {
            // Bind the view
            if (view != null)
            {
                Container.Bind<EnemyWaveSpawnerView>().FromInstance(view).AsSingle();
            }
            else
            {
                // note: provider resolves at inject time, not now
                Container.Bind<EnemyWaveSpawnerView>().FromComponentInHierarchy().AsSingle();
            }

            // Build the model from the bound view (no type-resolution for WaveConfig).
            Container.Bind<IEnemyWaveSpawnerModel>()
                .FromMethod(ctx =>
                {
                    var v = ctx.Container.Resolve<EnemyWaveSpawnerView>();
                    var w = v.Wave;
                    if (w == null)
                        throw new System.Exception("EnemyWaveSpawnerInstaller: WaveConfig is not assigned on EnemyWaveSpawnerView.");
                    return new EnemyWaveSpawnerModel(w, v.RandomSeed);
                })
                .AsSingle();

            // Presenter orchestrates spawning. NonLazy to catch issues early.
            Container.BindInterfacesAndSelfTo<EnemyWaveSpawnerPresenter>().AsSingle().NonLazy();
        }
    }
}
