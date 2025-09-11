using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Binds input → PlayerModel (state), model.MoveInput → PlayerView.Move (visuals),
    /// and enemy contacts → PlayerHealthModel.ApplyDamage.
    /// </summary>
    public sealed class PlayerPresenter : IInitializable
    {
        private readonly JoystickView m_joystickView;
        private readonly PlayerView m_playerView;
        private readonly IPlayerModel m_playerModel;
        private readonly IPlayerHealthModel m_healthModel;
        private readonly CompositeDisposable m_disposer;

        private static readonly TimeSpan k_HitCooldown = TimeSpan.FromMilliseconds(350);
        private const int k_DefaultContactDamage = 1; // fallback if enemy didn't set it

        public PlayerPresenter(
            JoystickView joystickView,
            PlayerView playerView,
            IPlayerModel playerModel,
            IPlayerHealthModel healthModel,
            CompositeDisposable disposer)
        {
            m_joystickView = joystickView;
            m_playerView = playerView;
            m_playerModel = playerModel;
            m_healthModel = healthModel;
            m_disposer = disposer;
        }

        public void Initialize()
        {
            // Input -> model
            m_joystickView.OnInput
                .Subscribe(m_playerModel.SetMoveInput)
                .AddTo(m_disposer);

            // Move every frame from latest input
            Observable.EveryUpdate()
                .Select(_ => m_playerModel.MoveInput.Value)
                .Subscribe(m_playerView.Move)
                .AddTo(m_disposer);

            // Movement state edges (optional)
            m_playerModel.StartedMoving.Subscribe(_ => { /* hook animations */ }).AddTo(m_disposer);
            m_playerModel.StoppedMoving.Subscribe(_ => { /* hook animations */ }).AddTo(m_disposer);

            // Enemy contact -> apply ENEMY'S damage
            m_playerView.EnemyCollided
                .ThrottleFirst(k_HitCooldown)
                .Subscribe(enemyView =>
                {
                    int damage = enemyView != null && enemyView.ContactDamage > 0
                        ? enemyView.ContactDamage
                        : k_DefaultContactDamage;

                    m_healthModel.ApplyDamage(damage);
                })
                .AddTo(m_disposer);
        }
    }
}
