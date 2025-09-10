using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        
        [SerializeField, Range(float.Epsilon, 5f)]
        private float _speed;

        public Vector2 Position => transform.position;

        public void Move(Vector2 direction)
        {
            var oldPosition = transform.position;
            transform.position = Vector3.Lerp(oldPosition, oldPosition + (Vector3)direction * _speed,
                Time.deltaTime);
        }

        public void UpdateHealth(float currentHealth)
        {
            _healthBar.DOValue(currentHealth, .2f).SetEase(Ease.OutSine);
        }
    }
}