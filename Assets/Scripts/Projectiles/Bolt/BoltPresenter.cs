using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltPresenter : ProjectilePresenter
    {
        private readonly Camera m_camera;
        private readonly Transform m_activeEnemiesRoot;

        // Tunables (consider exposing via a ScriptableObject later)
        private const float k_EdgePaddingWorld = 0.25f; // world-units “inside” the view rect
        private const float k_RicochetCooldown = 0.08f; // seconds between ricochets

        private float m_lastRicochetAt = -999f;

        public BoltPresenter(
            IBoltModel model,
            ProjectileView view,
            [InjectOptional] Camera camera,
            [Inject(Id = "ActiveEnemiesRoot")] Transform activeEnemiesRoot
        ) : base(model, view)
        {
            m_camera = camera != null ? camera : Camera.main;
            m_activeEnemiesRoot = activeEnemiesRoot;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Edge checks follow the moving camera every frame.
            Observable.EveryUpdate()
                .Subscribe(_ => TickRicochet())
                .AddTo(_disposer);
        }

        /// <summary>Call this from the spawner for a random initial direction.</summary>
        public void InitializeRandomMotion(Vector3 spawnPosition)
        {
            var rnd = UnityEngine.Random.insideUnitCircle.normalized;
            InitializeMotion(spawnPosition, new Vector3(rnd.x, rnd.y, 0f));
        }

        private void TickRicochet()
        {
            if (m_camera == null) return;

            var now = Time.time;
            if (now - m_lastRicochetAt < k_RicochetCooldown) return;

            var pos = GetPosition();

            if (m_camera.orthographic)
            {
                var halfH = m_camera.orthographicSize;
                var halfW = halfH * m_camera.aspect;
                var cpos = m_camera.transform.position;

                float left = cpos.x - halfW;
                float right = cpos.x + halfW;
                float bottom = cpos.y - halfH;
                float top = cpos.y + halfH;

                bool outside =
                    pos.x < left - k_EdgePaddingWorld ||
                    pos.x > right + k_EdgePaddingWorld ||
                    pos.y < bottom - k_EdgePaddingWorld ||
                    pos.y > top + k_EdgePaddingWorld;

                // Also trigger if we’re near an edge and moving outward.
                bool atEdgeMovingOut = false;
                var dir = GetDirection();
                if (!outside)
                {
                    if (pos.x <= left + k_EdgePaddingWorld) atEdgeMovingOut |= dir.x < 0f;
                    if (pos.x >= right - k_EdgePaddingWorld) atEdgeMovingOut |= dir.x > 0f;
                    if (pos.y <= bottom + k_EdgePaddingWorld) atEdgeMovingOut |= dir.y < 0f;
                    if (pos.y >= top - k_EdgePaddingWorld) atEdgeMovingOut |= dir.y > 0f;
                }

                if (outside || atEdgeMovingOut)
                {
                    RedirectTowardClosestEnemyOrRandom();

                    // Nudge back inside so we don’t instantly retrigger while camera keeps moving.
                    var p = pos;
                    p.x = Mathf.Clamp(p.x, left + k_EdgePaddingWorld, right - k_EdgePaddingWorld);
                    p.y = Mathf.Clamp(p.y, bottom + k_EdgePaddingWorld, top - k_EdgePaddingWorld);
                    _view.SetPosition(p);

                    m_lastRicochetAt = now;
                }
            }
            else
            {
                // Perspective fallback (keeps working with a moving camera)
                var vp = m_camera.WorldToViewportPoint(pos);
                bool outside = vp.z < 0f || vp.x < -0.02f || vp.x > 1.02f || vp.y < -0.02f || vp.y > 1.02f;
                if (outside)
                {
                    RedirectTowardClosestEnemyOrRandom();
                    m_lastRicochetAt = now;
                }
            }
        }

        private void RedirectTowardClosestEnemyOrRandom()
        {
            var target = FindClosestActiveEnemy();
            if (target != null)
            {
                var to = target.position - GetPosition();
                to.z = 0f;
                if (to.sqrMagnitude > 0.0001f)
                {
                    SetDirection(to.normalized);
                    return;
                }
            }

            // Fallback if no enemies
            var rnd = UnityEngine.Random.insideUnitCircle.normalized;
            SetDirection(new Vector3(rnd.x, rnd.y, 0f));
        }

        private Transform FindClosestActiveEnemy()
        {
            if (m_activeEnemiesRoot == null || m_activeEnemiesRoot.childCount == 0) return null;

            var pos = GetPosition();
            Transform closest = null;
            float bestSq = float.PositiveInfinity;

            for (int i = 0; i < m_activeEnemiesRoot.childCount; i++)
            {
                var child = m_activeEnemiesRoot.GetChild(i);
                if (!child.gameObject.activeInHierarchy) continue;

                var sq = (child.position - pos).sqrMagnitude;
                if (sq < bestSq)
                {
                    bestSq = sq;
                    closest = child;
                }
            }
            return closest;
        }
    }
}
