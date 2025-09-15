using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class KillCounterHUDInstaller : MonoInstaller
    {
        [Header("Kill Counter (optional override)")]
        [Tooltip("If left null, the view will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private EnemyKillCounterView killCounterView;

        public override void InstallBindings()
        {
            if (killCounterView)
                Container.BindInstance(killCounterView);
            else
                Container.Bind<EnemyKillCounterView>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IEnemyKillCounterModel>().To<EnemyKillCounterModel>().AsSingle();
            Container.BindInterfacesTo<EnemyKillCounterPresenter>().AsSingle().NonLazy();
        }
    }
}
