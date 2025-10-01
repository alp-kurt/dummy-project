using System;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        [Header("Collision")]
        [SerializeField] private LayerMask _enemyMask;

        public Vector2 Position => transform.position;
        public event Action<EnemyView> OnEnemyCollided;

        public void Translate(Vector3 delta) => transform.position += delta;

        private bool IsEnemyLayer(int layer) => (_enemyMask.value & (1 << layer)) != 0;

        private void OnTriggerEnter2D(Collider2D other) => HandleCollider(other.gameObject);
        private void OnCollisionEnter2D(Collision2D col) => HandleCollider(col.collider.gameObject);

        private void HandleCollider(GameObject hitGO)
        {
            if (!IsEnemyLayer(hitGO.layer)) return;

            // Same-GO fast path
            if (hitGO.TryGetComponent<EnemyView>(out var enemy))
            {
                OnEnemyCollided?.Invoke(enemy);
                return;
            }

            // Parent fallback
            var enemyFromParent = hitGO.GetComponentInParent<EnemyView>();
            if (enemyFromParent != null)
            {
                OnEnemyCollided?.Invoke(enemyFromParent);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Tiny validator: warn if mask empty or Collider2D missing
            if (_enemyMask.value == 0)
                Debug.LogWarning("[PlayerView] Enemy mask is empty. Select at least one layer.", this);

            if (!TryGetComponent<Collider2D>(out _))
                Debug.LogWarning("[PlayerView] Missing Collider2D.", this);
        }
#endif
    }
}
