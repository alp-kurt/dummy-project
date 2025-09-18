using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private enum State { Pooled, Active, OutOfScreen, Dead }

        private static readonly Subject<Unit> _anyDied = new Subject<Unit>();
        public static IObservable<Unit> AnyDied => _anyDied;

        // Timings
        private const float OffscreenDespawnSeconds = 1.5f;
        private const float DeathDespawnSeconds = 1.0f;

        private readonly IEnemyModel _model;
        private readonly EnemyView _view;
        private readonly Transform _player;
        private readonly EnemyStats _stats;

        private readonly CompositeDisposable _cd = new();
        private CompositeDisposable _stateScope = new();

        private State _state = State.Pooled;
        private bool _isOnScreen;

        [Inject]
        public EnemyPresenter(
            IEnemyModel model,
            PlayerView playerView,
            EnemyView view,
            EnemyStats stats)
        {
            _model = model;
            _view = view;
            _stats = stats;
            _player = playerView ? playerView.transform : null;

            // init model + visuals
            _model.Initialize(_stats);
            _view.SetVisual(_stats.Sprite, _stats.SpriteScale);
            _view.SetContactDamage(_stats.Damage);

            // allow projectiles to damage the model via the view
            _view.AttachModel(_model);


            // visibility -> Active/OutOfScreen
            _view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(on =>
                 {
                     _isOnScreen = on;
                     if (_state == State.Active && !on) Transition(State.OutOfScreen);
                     else if (_state == State.OutOfScreen && on) Transition(State.Active);
                 })
                 .AddTo(_cd);

            // death -> transition to Dead (despawn timer lives in presenter)
            _model.Died
                  .TakeUntilDisable(_view)
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

        // --- Presenter-local state machine (unchanged except Dead publish) ---
        private void Transition(State to)
        {
            if (_state == to) return;

            _stateScope.Dispose();
            _stateScope = new CompositeDisposable();

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
                    Observable.Timer(TimeSpan.FromSeconds(OffscreenDespawnSeconds))
                              .Where(_ => !_isOnScreen && _state == State.OutOfScreen)
                              .Subscribe(_ => Transition(State.Pooled))
                              .AddTo(_stateScope);
                    break;

                case State.Dead:
                    _model.SetCanMove(false);
                    _view.Stop();

                    _anyDied.OnNext(Unit.Default);

                    Observable.Timer(TimeSpan.FromSeconds(DeathDespawnSeconds))
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
