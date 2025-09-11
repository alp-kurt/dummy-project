using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Range(float.Epsilon, 5f)]
        private float speed = 1f;

        private readonly Subject<EnemyView> enemyCollided = new Subject<EnemyView>();
        public IObservable<EnemyView> EnemyCollided => enemyCollided;

        public Vector2 Position => transform.position;

        public void Move(Vector2 direction)
        {
            var from = transform.position;
            transform.position = Vector3.Lerp(from, from + (Vector3)direction * speed, Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponentInParent<EnemyView>();
            if (enemy != null) enemyCollided.OnNext(enemy);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var enemy = other.collider.GetComponentInParent<EnemyView>();
            if (enemy != null) enemyCollided.OnNext(enemy);
        }
    }
}
