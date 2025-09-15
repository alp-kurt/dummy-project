using UnityEngine;

namespace Scripts
{
    public sealed class PlayerPositionAdapter : IPlayerPosition
    {
        private readonly PlayerView _playerView;

        public PlayerPositionAdapter(PlayerView playerView)
        {
            _playerView = playerView;
        }

        public Vector3 Position => _playerView ? _playerView.transform.position : Vector3.zero;
    }
}
