using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public class ProjectilePresenter : IInitializable, IDisposable
    {
        protected readonly IProjectileModel _model;
        protected readonly ProjectileView _view;

        // One-time wiring for the presenter's entire lifetime.
        private readonly CompositeDisposable _staticDisposer = new();

        // Re-created on each spawn; torn down on despawn.
        private CompositeDisposable _spawnDisposer;

        protected readonly Subject<Unit> _despawnRequested = new Subject<Unit>();
        public IObservable<Unit> DespawnRequested => _despawnRequested;

        private Vector3 _direction = Vector3.right;
        private bool _isMotionInitialized;

        public ProjectileView View => _view;

        public ProjectilePresenter(IProjectileModel model, ProjectileView view)
        {
            _model = model;
            _view = view;
        }

        public virtual void Initialize()
        {
            _model.IsActiveRx
                .Subscribe(active => _view.SetActive(active))
                .AddTo(_staticDisposer);

            _view.SetSprite(_model.Sprite);

            // Route hits to damage delivery; guard with active.
            _view.HitTargets
                .Subscribe(target =>
                {
                    if (_model.IsActive)
                        target.ReceiveDamage(_model.Damage);
                })
                .AddTo(_staticDisposer);
        }

        public void InitializeMotion(Vector3 spawnPosition, Vector3 directionNormalized)
        {
            _view.SetPosition(spawnPosition);
            SetDirection(directionNormalized);

            if (_model is IHasLifetime life)
                life.ResetLifetime();

            _isMotionInitialized = true;

            // Recreate per-spawn disposable bucket
            _spawnDisposer?.Dispose();
            _spawnDisposer = new CompositeDisposable();

            // Per-spawn movement tick
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (!_model.IsActive || !_isMotionInitialized) return;
                    var delta = _direction * (_model.Speed * Time.deltaTime);
                    _view.Move(delta);
                })
                .AddTo(_spawnDisposer);

            _model.Activate();

            // Allow subclasses to add per-spawn work (ricochet, lifetime ticks, etc.)
            OnSpawned();
        }

        public void PrepareForDespawn()
        {
            _model.Deactivate();
            _isMotionInitialized = false;

            _spawnDisposer?.Dispose();
            _spawnDisposer = null;

            _view.SetActive(false);
        }

 
        protected virtual void OnSpawned() { }

        protected void AttachToSpawn(IDisposable d) => _spawnDisposer?.Add(d);

        protected void SetDirection(Vector3 dir)
        {
            _direction = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector3.right;
        }

        protected Vector3 GetPosition() => _view.CachedTransform.position;
        protected Vector3 GetDirection() => _direction;

        protected void RequestDespawn() => _despawnRequested.OnNext(Unit.Default);

        public virtual void Dispose()
        {
            _spawnDisposer?.Dispose();
            _staticDisposer.Dispose();
            _despawnRequested.OnCompleted();
        }
    }
}
