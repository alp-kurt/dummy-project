using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public interface IEnemyRegistry
    {
        EnemyView FindClosestVisible(Vector3 fromPosition);
    }

    public sealed class EnemyRegistry : IEnemyRegistry, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly HashSet<EnemyView> _active = new();

        public EnemyRegistry(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<EnemySpawnedSignal>(OnEnemySpawned);
            _signalBus.Subscribe<EnemyReturnedToPoolSignal>(OnEnemyReturnedToPool);
            _signalBus.Subscribe<EnemyDiedSignal>(OnEnemyDied);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemySpawnedSignal>(OnEnemySpawned);
            _signalBus.Unsubscribe<EnemyReturnedToPoolSignal>(OnEnemyReturnedToPool);
            _signalBus.Unsubscribe<EnemyDiedSignal>(OnEnemyDied);
            _active.Clear();
        }

        public EnemyView FindClosestVisible(Vector3 fromPosition)
        {
            EnemyView closest = null;
            float bestSq = float.PositiveInfinity;

            if (_active.Count == 0) return null;

            List<EnemyView> stale = null;
            foreach (var view in _active)
            {
                if (view == null)
                {
                    (stale ??= new List<EnemyView>()).Add(view);
                    continue;
                }

                if (!view.gameObject.activeInHierarchy) continue;
                if (!view.IsVisible) continue;

                float d2 = (view.Position - fromPosition).sqrMagnitude;
                if (d2 < bestSq)
                {
                    bestSq = d2;
                    closest = view;
                }
            }

            if (stale != null)
            {
                for (int i = 0; i < stale.Count; i++)
                    _active.Remove(stale[i]);
            }

            return closest;
        }

        private void OnEnemySpawned(EnemySpawnedSignal signal)
        {
            if (signal.View != null)
                _active.Add(signal.View);
        }

        private void OnEnemyReturnedToPool(EnemyReturnedToPoolSignal signal)
        {
            if (signal.View != null)
                _active.Remove(signal.View);
        }

        private void OnEnemyDied(EnemyDiedSignal signal)
        {
            if (signal.View != null)
                _active.Remove(signal.View);
        }
    }
}
