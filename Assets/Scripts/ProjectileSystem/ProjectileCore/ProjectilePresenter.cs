using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Base presenter: wires model ↔ view, handles activation, and per-spawn movement.
    /// Subclasses (e.g., BoltPresenter) can attach extra per-spawn logic via OnSpawned()/AttachToSpawn().
    /// </summary>
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

        /// <summary> One-time wiring; safe for pooling. </summary>
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

        /// <summary>
        /// Called each time the projectile is spawned.
        /// Sets position/direction, resets lifetime (if any), recreates per-spawn subscriptions, and activates the model.
        /// </summary>
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

        /// <summary>
        /// Called just before returning the projectile to the pool.
        /// </summary>
        public void PrepareForDespawn()
        {
            _model.Deactivate();
            _isMotionInitialized = false;

            _spawnDisposer?.Dispose();
            _spawnDisposer = null;

            _view.SetActive(false);
        }

        /// <summary>
        /// Subclasses override to attach per-spawn logic. Called at the end of InitializeMotion().
        /// </summary>
        protected virtual void OnSpawned() { }

        /// <summary>
        /// Helper for subclasses to add disposables to the current spawn bucket.
        /// </summary>
        protected void AttachToSpawn(IDisposable d) => _spawnDisposer?.Add(d);

        /// <summary> Set movement direction; falls back to +X if zero. </summary>
        protected void SetDirection(Vector3 dir)
        {
            _direction = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector3.right;
        }

        /// <summary> Accessors for subclasses. </summary>
        protected Vector3 GetPosition() => _view.CachedTransform.position;
        protected Vector3 GetDirection() => _direction;

        /// <summary>
        /// Subclasses call this to ask the pool/handle to despawn the projectile.
        /// </summary>
        protected void RequestDespawn() => _despawnRequested.OnNext(Unit.Default);

        public virtual void Dispose()
        {
            _spawnDisposer?.Dispose();
            _staticDisposer.Dispose();
            _despawnRequested.OnCompleted();
        }
    }
}
