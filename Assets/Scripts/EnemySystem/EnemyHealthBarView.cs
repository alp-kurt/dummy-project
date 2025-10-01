using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    public sealed class EnemyHealthBarView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")]
        [SerializeField, Range(0f, 1f)] private float _valueTween = 0.05f;

        private Tween _valueTw, _fadeTw;
        private readonly CompositeDisposable _cd = new();

        private EnemyView _view;
        private IEnemyModel _model;

        private void Awake()
        {
            if (!_slider) _slider = GetComponentInChildren<Slider>(true);
            if (!_canvasGroup) _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            _view = GetComponentInParent<EnemyView>(true);
        }

        private void OnEnable()
        {
            _cd.Clear();
            TryBindOrWait();
        }

        private void OnDisable()
        {
            KillTweens();
            _cd.Clear();
        }

        private void TryBindOrWait()
        {
            if (_view == null)
            {
                SetVisible(false);
                return;
            }

            // Immediate bind if model is already attached
            if (_view.Model != null)
            {
                Bind(_view.Model);
                return;
            }

            // Late-bind ONCE when presenter attaches the model after pooling activation
            Observable.EveryUpdate()
                      .First(_ => _view != null && _view.Model != null)
                      .Subscribe(_ => Bind(_view.Model))
                      .AddTo(_cd);

            SetVisible(false);
        }

        private void Bind(IEnemyModel model)
        {
            _model = model;
            if (_model == null)
            {
                SetVisible(false);
                return;
            }

            // Initial set
            float initial = (_model.MaxHealth <= 0)
                ? 0f
                : Mathf.Clamp01((float)_model.CurrentHealth.Value / _model.MaxHealth);
            SetHealth01Tweened(initial);
            SetVisible(true);

            // Reactive updates
            _model.CurrentHealth
                  .Select(hp => _model.MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)hp / _model.MaxHealth))
                  .DistinctUntilChanged()
                  .Subscribe(SetHealth01Tweened)
                  .AddTo(_cd);

            // Hide on death
            _model.Died
                  .Take(1)
                  .Subscribe(__ => SetVisible(false))
                  .AddTo(_cd);
        }

        private void SetHealth01Tweened(float v)
        {
            if (!_slider) return;
            _valueTw?.Kill(true);
            _valueTw = _slider.DOValue(v, _valueTween).SetUpdate(true);
        }

        private void SetVisible(bool visible)
        {
            _fadeTw?.Kill(true);

            if (_canvasGroup)
            {
                _fadeTw = _canvasGroup.DOFade(visible ? 1f : 0f, 0.15f).SetUpdate(true);
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
            else if (_slider)
            {
                _slider.gameObject.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void KillTweens()
        {
            _valueTw?.Kill(true);
            _fadeTw?.Kill(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_slider) _slider = GetComponentInChildren<Slider>(true);
            if (!_canvasGroup) _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            _valueTween = Mathf.Clamp01(_valueTween);
        }
#endif
    }
}
