using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class KillCounterInstaller : MonoInstaller
    {
        [SerializeField] private EnemyKillCounterView killCounterView;

        public override void InstallBindings()
        {

            // View
            if (killCounterView)
                Container.BindInstance(killCounterView);
            else
                Container.Bind<EnemyKillCounterView>().FromComponentInHierarchy().AsSingle();

            // MVP
            Container.Bind<IEnemyKillCounterModel>().To<EnemyKillCounterModel>().AsSingle();
            Container.BindInterfacesTo<EnemyKillCounterPresenter>().AsSingle().NonLazy();
        }
    }
}