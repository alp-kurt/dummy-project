using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts
{
    public sealed class PlayerHealthBarView : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Optional explicit refs (auto-detects if left empty)")]
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;

        private IPlayerModel _player;

        private readonly CompositeDisposable _cd = new();

        [Inject] void Construct(IPlayerModel player) => _player = player;

        void Awake()
        {
            // Auto-detect if not wired
            if (!_slider) _slider = GetComponentInChildren<Slider>(true);
            if (!_fillImage) _fillImage = GetComponentInChildren<Image>(true);
        }

        public void Initialize()
        {
            // current / max → normalized 0..1
            _player.CurrentHealth
                .Select(hp => _player.MaxHealth <= 0f ? 0f : Mathf.Clamp01(hp / _player.MaxHealth))
                .DistinctUntilChanged()
                .Subscribe(SetNormalized)
                .AddTo(_cd);
        }

        public void Dispose()
        {
            _cd.Dispose();
        }

        private void SetNormalized(float v)
        {
            if (_slider) _slider.value = v;
            if (_fillImage && _fillImage.type == Image.Type.Filled) _fillImage.fillAmount = v;
        }
    }
}
