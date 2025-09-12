using System;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        public Vector2 Position => transform.position;

        public event Action<EnemyView> OnEnemyCollided;

        public void Translate(Vector3 delta) => transform.position += delta;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponentInParent<EnemyView>();
            if (enemy != null) OnEnemyCollided?.Invoke(enemy);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var enemy = other.collider.GetComponentInParent<EnemyView>();
            if (enemy != null) OnEnemyCollided?.Invoke(enemy);
        }
    }
}
