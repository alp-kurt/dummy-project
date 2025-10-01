using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts
{
    public class JoystickView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _stick;
        [SerializeField] private RectTransform _stickParent;

        private readonly Subject<Vector2> _onInput = new();
        private bool _isDragging;
        
        public IObservable<Vector2> OnInput => _onInput;

        private void Update()
        {
            if (_isDragging)
            {
                var stickPosition = _stickParent.InverseTransformPoint(Input.mousePosition);
                var stickParentRect = _stickParent.rect;

                var radius = stickParentRect.width / 2;
                var distance = stickPosition.magnitude;
                if (distance > radius)
                {
                    stickPosition = stickPosition.normalized * radius;
                }
                
                _stick.rectTransform.localPosition = stickPosition;
                _onInput.OnNext(_stick.rectTransform.localPosition / radius);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetStick();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            ResetStick();
        }
        
        private void ResetStick()
        {
            _isDragging = false;
            _stick.rectTransform.localPosition = Vector2.zero;
            _onInput.OnNext(Vector2.zero);
        }
    }
}