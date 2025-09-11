using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Scripts
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        [Header("UI (World Space Slider on a child)")]
        [SerializeField] private Slider healthBar;            
        [SerializeField] private float healthTweenSeconds = 0.2f;

        [Header("Movement")]
        [SerializeField, Range(float.Epsilon, 5f)]
        private float speed = 1f;

        // Emits when an EnemyView collides with the player (you already use this)
        private readonly Subject<EnemyView> enemyCollided = new Subject<EnemyView>();
        public IObservable<EnemyView> EnemyCollided => enemyCollided;

        public Vector2 Position => transform.position;

        private void Awake()
        {
            // Auto-find the slider if not assigned (looks in children, even if inactive)
            if (healthBar == null)
                healthBar = GetComponentInChildren<Slider>(true);

            if (healthBar != null)
            {
                healthBar.minValue = 0f;
                healthBar.maxValue = 1f;
                // ensure initial value shown (in case model subscribes after Awake)
                if (healthBar.value != 1f) healthBar.value = 1f;
            }
            else
            {
                Debug.LogWarning("[PlayerView] Health Slider not found. Assign it on the Player prefab.", this);
            }
        }

        public void Move(Vector2 direction)
        {
            var oldPos = transform.position;
            transform.position = Vector3.Lerp(oldPos, oldPos + (Vector3)direction * speed, Time.deltaTime);
        }

        /// <summary>
        /// Update the health bar (expects normalized 0..1).
        /// </summary>
        public void UpdateHealth(float currentHealth01)
        {
            if (healthBar == null) return;

            float clamped = Mathf.Clamp01(currentHealth01);

            // Tween to soften jumps
            healthBar.DOValue(clamped, healthTweenSeconds)
                     .SetEase(Ease.OutSine);
        }

        // --- Collision forwarding (unchanged) ---
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
