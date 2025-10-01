using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class KillCounterHUDInstaller : MonoInstaller
    {
        [Header("Kill Counter (optional)")]
        [Tooltip("If left null, the view will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private EnemyKillCounterView _killCounterView;

        public override void InstallBindings()
        {
            // View
            if (_killCounterView)
                Container.Bind<EnemyKillCounterView>().FromInstance(_killCounterView).AsSingle();
            else
                Container.Bind<EnemyKillCounterView>().FromComponentInHierarchy().AsSingle();

            // Model (single source of truth)
            Container.Bind<IEnemyKillCounterModel>()
                     .To<EnemyKillCounterModel>()
                     .AsSingle()
                     .IfNotBound();

            // Presenter (IInitializable -> Initialize() will be called)
            Container.BindInterfacesTo<EnemyKillCounterPresenter>()
                     .AsSingle()
                     .NonLazy();
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
