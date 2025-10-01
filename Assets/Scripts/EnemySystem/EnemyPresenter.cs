using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private enum State { Pooled, OutOfScreen, Active, Dead }

        private static readonly Subject<Unit> _anyDied = new();
        public static IObservable<Unit> AnyDied => _anyDied;

        // Timings
        private static readonly TimeSpan OffscreenDespawnDelay = TimeSpan.FromSeconds(1.5f);
        private static readonly TimeSpan DeathDespawnDelay = TimeSpan.FromSeconds(1.0f);

        private readonly EnemyModel _model;
        private readonly EnemyView _view;
        private readonly Transform _player;
        private readonly EnemyStats _stats;

        private readonly ReactiveProperty<State> _state = new(State.Pooled);
        private readonly Subject<Unit> _returnedToPool = new();
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public EnemyPresenter(EnemyModel model, EnemyView view, EnemyStats stats, Transform player)
        {
            _model = model;
            _view = view;
            _stats = stats;
            _player = player;

            // init model + visuals
            _model.Initialize(_stats);
            _view.SetVisual(_stats.Sprite, _stats.SpriteScale);
            _view.SetContactDamage(_stats.Damage);

            // allow projectiles to damage the model via the view
            _view.AttachModel(_model);

            BindStateMachine();
            BindMovement();
        }

        public IObservable<Unit> ReturnedToPool => _returnedToPool.AsObservable();

        // --- Pool API ---
        public void SpawnFromPool()
        {
            _model.ResetForSpawn();
            _view.SetContactDamage(_stats.Damage);
            _view.SetActive(true);

            SetState(State.OutOfScreen);
            if (_view.IsVisible)
            {
                SetState(State.Active);
            }
        }

        public void DespawnToPool()
        {
            SetState(State.Pooled);
        }

        // --- Presenter-local state machine ---
        private void BindStateMachine()
        {

            _state
          .Subscribe(OnStateChanged)
          .AddTo(_disposables);

            _state
                .Skip(1)
                .Where(state => state == State.Pooled)
                .Subscribe(_ => _returnedToPool.OnNext(Unit.Default))
                .AddTo(_disposables);

            _view.VisibilityChanged
                 .WithLatestFrom(_state, (visible, state) => (visible, state))
                 .Where(pair => pair.state == State.Active || pair.state == State.OutOfScreen)
                 .Select(pair => pair.visible ? State.Active : State.OutOfScreen)
                 .Subscribe(SetState)
                 .AddTo(_disposables);

            _state
                .Where(state => state == State.OutOfScreen)
                .Select(_ =>
                    Observable.Timer(OffscreenDespawnDelay)
                              .TakeUntil(_state.Where(s => s != State.OutOfScreen)))
                .Switch()
                .Subscribe(_ => SetState(State.Pooled))
                .AddTo(_disposables);

            _model.Died
                  .Subscribe(_ => SetState(State.Dead))
                  .AddTo(_disposables);

            _state
                .Where(state => state == State.Dead)
                .Select(_ =>
                    Observable.Timer(DeathDespawnDelay)
                              .TakeUntil(_state.Where(s => s != State.Dead)))
                .Switch()
                .Subscribe(_ => SetState(State.Pooled))
                .AddTo(_disposables);
        }

        private void BindMovement()
        {
            Observable.EveryFixedUpdate()
                      .WithLatestFrom(_state, (_, state) => state)
                      .Where(state => state == State.Active || state == State.OutOfScreen)
                      .Subscribe(_ => TickMovement(Time.fixedDeltaTime))
                      .AddTo(_disposables);
        }

        private void OnStateChanged(State state)
        {
            switch (state)
            {
                case State.Pooled:
                    _model.SetCanMove(false);
                    _view.Stop();
                    _view.SetActive(false);
                    break;

                case State.OutOfScreen:
                    _model.SetCanMove(true);
                    if (!_view.IsActive) _view.SetActive(true);
                    break;

                case State.Active:
                    _model.SetCanMove(true);
                    if (!_view.IsActive) _view.SetActive(true);
                    break;

                case State.Dead:
                    _model.SetCanMove(false);
                    _view.Stop();
                    _anyDied.OnNext(Unit.Default);
                    break;
            }
        }

        private void TickMovement(float fixedDt)
        {
            if (_player == null || !_model.CanMove.Value)
            {
                _view.ApplyVelocityFixed(Vector2.zero, fixedDt);
                return;
            }

            var toPlayer = (Vector2)(_player.position - _view.Position);
            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                _view.ApplyVelocityFixed(Vector2.zero, fixedDt);
                return;
            }

            var velocity = toPlayer.normalized * _model.MoveSpeed;
            _view.ApplyVelocityFixed(velocity, fixedDt);
        }

        private void SetState(State next)
        {
            if (_state.Value == next) return;
            _state.Value = next;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _state.Dispose();
            _returnedToPool.OnCompleted();
            _returnedToPool.Dispose();
        }

    }
}
