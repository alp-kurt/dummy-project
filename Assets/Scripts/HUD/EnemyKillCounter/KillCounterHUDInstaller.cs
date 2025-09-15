using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class KillCounterHUDInstaller : MonoInstaller
    {
        [Header("Kill Counter (optional override)")]
        [Tooltip("If left null, the view will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private EnemyKillCounterView _killCounterView;

        public override void InstallBindings()
        {
            if (_killCounterView)
                Container.BindInstance(_killCounterView);
            else
                Container.Bind<EnemyKillCounterView>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IEnemyKillCounterModel>().To<EnemyKillCounterModel>().AsSingle();
            Container.BindInterfacesTo<EnemyKillCounterPresenter>().AsSingle().NonLazy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_killCounterView)
                Debug.Log($"[{nameof(KillCounterHUDInstaller)}] View not set; will resolve via hierarchy.", this);
        }
#endif
    }
}
