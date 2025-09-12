using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Inject] private WaveConfig m_wave;
        [Inject] private EnemyPool m_pool; // or inject EnemyViewMemoryPool when you migrate

        [SerializeField] private Camera m_camera;
        [SerializeField] private int m_randomSeed = 12345;

        private System.Random m_rng;
        private CancellationTokenSource m_cts;

        private void Awake()
        {
            if (!m_camera) m_camera = Camera.main;
            m_rng = new System.Random(m_randomSeed);
        }

        private void OnEnable()
        {
            m_cts = new CancellationTokenSource();
            RunLoop(m_cts.Token).Forget();
        }

        private void OnDisable()
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_cts = null;
        }

        private async UniTaskVoid RunLoop(CancellationToken token)
        {
            if (m_wave == null || m_pool == null || m_camera == null) return;

            var wait = TimeSpan.FromSeconds(Mathf.Max(0.05f, m_wave.spawnIntervalSeconds));

            while (!token.IsCancellationRequested)
            {
                int count = Mathf.Max(1, m_wave.densityPerTick);
                for (int i = 0; i < count; i++)
                {
                    var stats = m_wave.PickRandomStats(m_rng);
                    if (stats == null) continue;

                    var handle = m_pool.Get(stats);
                    PlaceOffScreen(handle.View.transform, m_camera, m_wave.offscreenPadding);
                }

                await UniTask.Delay(wait, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        private static void PlaceOffScreen(Transform t, Camera cam, float padding)
        {
            float z = Mathf.Abs(cam.transform.position.z - t.position.z);
            Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));
            Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, z));

            float left = Mathf.Min(bl.x, tr.x);
            float right = Mathf.Max(bl.x, tr.x);
            float bottom = Mathf.Min(bl.y, tr.y);
            float top = Mathf.Max(bl.y, tr.y);

            int side = UnityEngine.Random.Range(0, 4);
            float x, y;
            switch (side)
            {
                case 0: x = left - padding; y = UnityEngine.Random.Range(bottom, top); break;
                case 1: x = right + padding; y = UnityEngine.Random.Range(bottom, top); break;
                case 2: x = UnityEngine.Random.Range(left, right); y = bottom - padding; break;
                default: x = UnityEngine.Random.Range(left, right); y = top + padding; break;
            }
            t.position = new Vector3(x, y, t.position.z);
        }
    }
}
