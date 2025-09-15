using System;
using UnityEngine;
using Zenject;
using UniRx;

namespace Scripts
{
    public sealed class PlayerPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly JoystickView m_joystickView;
        private readonly PlayerView m_view;
        private readonly IPlayerModel m_model;
        private readonly IPlayerHealthModel m_health;

        private readonly TimeSpan m_hitCooldown = TimeSpan.FromMilliseconds(350);
        private float m_nextHitTime;
        private bool m_disposed;

        private IDisposable _inputSub;
        private Action<EnemyView> _onEnemyCollidedHandler;

        public PlayerPresenter(JoystickView joystickView, PlayerView view, IPlayerModel model, IPlayerHealthModel health)
        {
            m_joystickView = joystickView;
            m_view = view;
            m_model = model;
            m_health = health;
        }

        public void Initialize()
        {
            _inputSub = m_joystickView.OnInput
                .DistinctUntilChanged()
                .Subscribe(m_model.SetMoveInput);

            _onEnemyCollidedHandler = e =>
            {
                if (Time.time < m_nextHitTime) return;
                m_nextHitTime = Time.time + (float)m_hitCooldown.TotalSeconds;

                var dmg = (e != null && e.ContactDamage > 0) ? e.ContactDamage : 1;
                m_health.ReceiveDamage(dmg);
            };

            m_view.OnEnemyCollided += _onEnemyCollidedHandler;
        }

        public void Tick()
        {
            var delta = m_model.Step(Time.deltaTime);
            m_view.Translate(delta);
        }

        public void Dispose()
        {
            if (m_disposed) return;

            _inputSub?.Dispose();

            if (_onEnemyCollidedHandler != null)
            {
                m_view.OnEnemyCollided -= _onEnemyCollidedHandler;
                _onEnemyCollidedHandler = null;
            }

            m_disposed = true;
        }
    }
}
