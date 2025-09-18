using UniRx;
using UnityEngine;
using DG.Tweening;

namespace Scripts
{
    /// <summary>
    /// On every EnemyModel.Damaged, applies a DOPunchScale to the visual.
    /// Attach to the enemy prefab. If Target is empty, grabs the first SpriteRenderer's transform, else self.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHitFx : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Punch Settings")]
        [Tooltip("Scale amount to subtract on hit (X/Y). e.g. 0.15 => scales to 0.85 briefly.")]
        [SerializeField, Min(0f)] private float _punchAmount = 0.15f;

        [Tooltip("Total punch duration (seconds).")]
        [SerializeField, Min(0.02f)] private float _duration = 0.12f;

        [Tooltip("How snappy the punch oscillation is.")]
        [SerializeField, Min(1)] private int _vibrato = 6;

        [Tooltip("0 = no overshoot, 1 = lots of overshoot.")]
        [SerializeField, Range(0f, 1f)] private float _elasticity = 0.0f;

        private EnemyView _view;
        private IEnemyModel _model;
        private readonly CompositeDisposable _cd = new();

        private void Awake()
        {
            _view = GetComponentInParent<EnemyView>(true);
            if (!_target)
            {
                var sr = GetComponentInChildren<SpriteRenderer>(true);
                _target = sr ? sr.transform : transform;
            }
        }

        private void OnEnable()
        {
            _cd.Clear();

            // bind immediately or wait for presenter to attach model (pool-friendly)
            if (_view != null && _view.Model != null) Bind(_view.Model);
            else
            {
                Observable.EveryUpdate()
                    .First(_ => _view != null && _view.Model != null)
                    .Subscribe(_ => Bind(_view.Model))
                    .AddTo(_cd);
            }
        }

        private void OnDisable()
        {
            _target?.DOKill();
            _cd.Clear();
        }

        private void Bind(IEnemyModel model)
        {
            _model = model;
            if (_model == null) return;

            _model.Damaged
                  .Subscribe(__ => PlayPunch())
                  .AddTo(_cd);

            _model.Died
                  .Take(1)
                  .Subscribe(__ => _target?.DOKill())
                  .AddTo(_cd);
        }

        private void PlayPunch()
        {
            if (!_target) return;

            // Kill any current punch so hits don't stack ugly
            _target.DOKill(complete: false);

            // Negative punch brings scale down then returns to baseline
            var punch = new Vector3(-_punchAmount, -_punchAmount, 0f);
            _target.DOPunchScale(punch, _duration, _vibrato, _elasticity)
                   .SetUpdate(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_target)
            {
                var sr = GetComponentInChildren<SpriteRenderer>(true);
                if (sr) _target = sr.transform;
            }
        }
#endif
    }
}
