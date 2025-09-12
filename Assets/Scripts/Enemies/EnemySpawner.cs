using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [Inject] private WaveConfig m_wave;
        [Inject] private EnemyPool m_pool;

        [Header("Required")]
        [SerializeField] private Camera cam;

        [Header("Randomness")]
        [SerializeField] private int randomSeed = 12345;

        private System.Random m_rng;
        private CancellationTokenSource m_cts;

        // ISpawner<EnemyView>
        private readonly Subject<EnemyView> _spawned = new Subject<EnemyView>();
        public IObservable<EnemyView> Spawned => _spawned;

        private void OnEnable()
        {
            if (!m_wave) throw new NullReferenceException($"{nameof(EnemySpawner)}: WaveConfig not injected.");
            if (!m_pool) throw new NullReferenceException($"{nameof(EnemySpawner)}: EnemyPool not injected.");
            if (!cam) throw new NullReferenceException($"{nameof(EnemySpawner)}: Camera is not assigned.");

            m_rng = new System.Random(randomSeed);
            Start();
        }

        private void OnDisable() => Stop();

        // ---- ISpawner API --------------------------------------------------

        public EnemyView SpawnOne()
        {
            var stats = m_wave.PickRandomStats(m_rng);
            if (!stats) return null;

            var view = m_pool.Get(stats);
            PlaceOffScreen(view.transform, cam, m_wave.offscreenPadding);
            _spawned.OnNext(view);
            return view;
        }

        public void Start()
        {
            if (m_cts != null) return; // already running
            m_cts = new CancellationTokenSource();
            RunLoop(m_cts.Token).Forget();
        }

        public void Stop()
        {
            m_cts?.Cancel();
            m_cts?.Dispose();
            m_cts = null;
        }

        // ---- Loop & helpers -----------------------------------------------

        private async UniTaskVoid RunLoop(CancellationToken token)
        {
            var wait = TimeSpan.FromSeconds(Mathf.Max(0.05f, m_wave.spawnIntervalSeconds));

            while (!token.IsCancellationRequested)
            {
                int count = Mathf.Max(1, m_wave.densityPerTick);
                for (int i = 0; i < count; i++)
                {
                    if (token.IsCancellationRequested) break;
                    SpawnOne();
                }

                await UniTask.Delay(wait, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
            }
        }

        private static void PlaceOffScreen(Transform t, Camera camera, float padding)
        {
            float z = Mathf.Abs(camera.transform.position.z - t.position.z);
            Vector3 bl = camera.ViewportToWorldPoint(new Vector3(0f, 0f, z));
            Vector3 tr = camera.ViewportToWorldPoint(new Vector3(1f, 1f, z));

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
