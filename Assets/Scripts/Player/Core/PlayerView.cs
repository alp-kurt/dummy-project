using System;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private string enemyLayerName = "Enemy";
        private int _enemyLayer;

        public Vector2 Position => transform.position;

        public event Action<EnemyView> OnEnemyCollided;

        public void Translate(Vector3 delta) => transform.position += delta;

        private void Awake()
        {
            _enemyLayer = LayerMask.NameToLayer(enemyLayerName);
            if (_enemyLayer == -1)
            {
                Debug.LogWarning($"[PlayerView] Enemy layer '{enemyLayerName}' not found. Check Project Settings → Tags and Layers.", this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => HandleCollider(other.gameObject, other);
        private void OnCollisionEnter2D(Collision2D other) => HandleCollider(other.collider.gameObject, other.collider);

        private void HandleCollider(GameObject hitGO, Component rawCollider)
        {
            // Respect your physics matrix: process only Enemy layer contacts
            if (_enemyLayer != -1 && hitGO.layer != _enemyLayer) return;

            // Fast-path: EnemyView on the same GO as the collider
            if (hitGO.TryGetComponent<EnemyView>(out var enemy))
            {
                OnEnemyCollided?.Invoke(enemy);
                return;
            }

            // Fallback: if collider is on a child, EnemyView might be on the parent
            var enemyFromParent = hitGO.GetComponentInParent<EnemyView>();
            if (enemyFromParent != null)
            {
                OnEnemyCollided?.Invoke(enemyFromParent);
            }
        }
    }
}