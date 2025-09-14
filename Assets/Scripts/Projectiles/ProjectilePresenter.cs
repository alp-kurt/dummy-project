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
        protected readonly CompositeDisposable _disposer = new();
        protected Vector3 GetDirection() => _direction;

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
                .AddTo(_disposer);

            _view.SetSprite(_model.Sprite);

            _view.HitTargets
                .Subscribe(dmg =>
                {
                    if (_model.IsActive)
                        dmg.ReceiveDamage(_model.Damage);
                })
                .AddTo(_disposer);

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (!_model.IsActive || !_isMotionInitialized) return;
                    var delta = _direction * (_model.Speed * Time.deltaTime);
                    _view.Move(delta);
                })
                .AddTo(_disposer);
        }

        public void InitializeMotion(Vector3 spawnPosition, Vector3 directionNormalized)
        {
            _view.SetPosition(spawnPosition);
            _direction = directionNormalized.sqrMagnitude > 0f
                ? directionNormalized.normalized
                : Vector3.right;

            _isMotionInitialized = true;
            _model.Activate();
        }

        public void PrepareForDespawn()
        {
            _model.Deactivate();
            _view.SetActive(false);
            _isMotionInitialized = false;
            Dispose();
        }

        public virtual void Dispose()
        {
            _disposer.Dispose();
        }

        protected void SetDirection(Vector3 dir)
        {
            if (dir.sqrMagnitude > 0f)
                _direction = dir.normalized;
        }

        protected Vector3 GetPosition()
        {
            return _view.CachedTransform.position;
        }

    }
}
