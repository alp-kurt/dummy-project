using System;
using UniRx;
using UnityEngine;
using Zenject;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private enum State { Pooled, Active, OutOfScreen, Dead }

        private readonly IEnemyModel _model;
        private readonly EnemyView _view;
        private readonly Transform _player;
        private readonly EnemyStats _stats;
        private readonly IEnemyDeathStream _deathBus;

        private readonly CompositeDisposable _cd = new();
        private CompositeDisposable _stateScope = new();

        private State _state = State.Pooled;
        private bool _isOnScreen;

        [Inject]
        public EnemyPresenter(
            IEnemyModel model,
            PlayerView playerView,
            EnemyView view,
            EnemyStats stats,
            IEnemyDeathStream deathBus)
        {
            _model = model;
            _view = view;
            _stats = stats;
            _player = playerView ? playerView.transform : null;
            _deathBus = deathBus;

            // init model + visuals
            _model.Initialize(_stats);
            _view.SetVisual(_stats.Sprite, _stats.SpriteScale);
            _view.SetContactDamage(_stats.Damage);

            // let projectiles damage the model via the view
            _view.AttachModel(_model);

            // subscribe hit fx
            _model.Damaged.Subscribe(_ =>
            {
                var t = _view.GetComponentInChildren<SpriteRenderer>(true)?.transform ?? _view.transform;
                t.DOKill(false);
                t.DOPunchScale(new Vector3(-0.15f, -0.15f, 0f), 0.12f, 6, 0f).SetUpdate(true);
            }).AddTo(_view);

            // view visibility -> keep a simple flag and transition between Active/OutOfScreen
            _view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(on =>
                 {
                     _isOnScreen = on;
                     if (_state == State.Active && !on) Transition(State.OutOfScreen);
                     else if (_state == State.OutOfScreen && on) Transition(State.Active);
                     // ignore in Pooled/Dead
                 })
                 .AddTo(_cd);

            _model.Died
                  .TakeUntilDisable(_view) // if view gets killed, stop listening
                  .Subscribe(_ => Transition(State.Dead))
                  .AddTo(_cd);

            // fixed-step movement
            Observable.EveryFixedUpdate()
                      .Subscribe(_ => TickFixed(Time.fixedDeltaTime))
                      .AddTo(_cd);
        }

        public IObservable<Unit> ReturnedToPool => _model.ReturnedToPool;

        public void Dispose()
        {
            _stateScope.Dispose();
            _cd.Dispose();
        }

        // --- Pool API ---
        public void SpawnFromPool()
        {
            _model.ResetForSpawn();
            _view.SetContactDamage(_stats.Damage);
            _view.SetActive(true);

            _isOnScreen = _view.IsVisible;
            Transition(State.OutOfScreen); 
            if (_isOnScreen) Transition(State.Active);
        }

        public void DespawnToPool()
        {
            _view.Stop();
            _view.SetActive(false);
            Transition(State.Pooled);
        }

        // --- Presenter-local state machine ---
        private void Transition(State to)
        {
            if (_state == to) return;

            // exit scope
            _stateScope.Dispose();
            _stateScope = new UniRx.CompositeDisposable();

            // enter
            switch (to)
            {
                case State.Pooled:
                    _model.SetCanMove(false);
                    _model.NotifyReturnedToPool();
                    break;

                case State.Active:
                    _model.SetCanMove(true);
                    break;

                case State.OutOfScreen:
                    _model.SetCanMove(true);
                    UniRx.Observable.Timer(TimeSpan.FromSeconds(1.5f))
                        .Where(_ => !_isOnScreen && _state == State.OutOfScreen)
                        .Subscribe(_ => Transition(State.Pooled))
                        .AddTo(_stateScope);
                    break;

                case State.Dead:
                    _model.SetCanMove(false);
                    _deathBus.Publish();
                    _view.Stop();

                    UniRx.Observable.Timer(TimeSpan.FromSeconds(1.0f))
                        .Where(_ => _state == State.Dead)
                        .Subscribe(_ => Transition(State.Pooled))
                        .AddTo(_stateScope);
                    break;
            }

            _state = to;
        }


        private void TickFixed(float fixedDt)
        {
            var allowMove = (_state == State.Active || _state == State.OutOfScreen) && _model.CanMove.Value;
            if (!allowMove || _player == null)
            {
                _view.ApplyVelocityFixed(Vector2.zero, fixedDt);
                return;
            }

            var dir = (_player.position - _view.Position);
            if (dir.sqrMagnitude <= 0.0001f)
            {
                _view.ApplyVelocityFixed(Vector2.zero, fixedDt);
                return;
            }

            var vel = (Vector2)dir.normalized * _model.MoveSpeed;
            _view.ApplyVelocityFixed(vel, fixedDt);
        }
    }
}
