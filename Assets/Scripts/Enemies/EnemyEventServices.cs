
using UnityEngine;

namespace Scripts
{
    public interface IEnemyUiService
    {
        void OnDamaged(EnemyDamagedEvent e, Vector3 worldPos);
        void OnDied(EnemyDiedEvent e, Vector3 worldPos);
        void OnReturned(EnemyReturnedToPoolEvent e, Vector3 worldPos);
    }

    public interface IEnemySfxService
    {
        void OnDamaged(EnemyDamagedEvent e, Vector3 worldPos);
        void OnDied(EnemyDiedEvent e, Vector3 worldPos);
        void OnReturned(EnemyReturnedToPoolEvent e, Vector3 worldPos);
    }

    public interface IEnemyAnalyticsService
    {
        void OnDamaged(EnemyDamagedEvent e);
        void OnDied(EnemyDiedEvent e);
        void OnReturned(EnemyReturnedToPoolEvent e);
    }

    public sealed class EnemyUiLogger : IEnemyUiService
    {
        public void OnDamaged(EnemyDamagedEvent e, Vector3 p) => Debug.Log($"[UI] Enemy {e}");
        public void OnDied(EnemyDiedEvent e, Vector3 p) => Debug.Log($"[UI] {e}");
        public void OnReturned(EnemyReturnedToPoolEvent e, Vector3 p) => Debug.Log($"[UI] {e}");
    }

    public sealed class EnemySfxLogger : IEnemySfxService
    {
        public void OnDamaged(EnemyDamagedEvent e, Vector3 p) => Debug.Log($"[SFX] Hit at {p}");
        public void OnDied(EnemyDiedEvent e, Vector3 p) => Debug.Log($"[SFX] Death at {p}");
        public void OnReturned(EnemyReturnedToPoolEvent e, Vector3 p) => Debug.Log($"[SFX] Despawn at {p}");
    }

    public sealed class EnemyAnalyticsLogger : IEnemyAnalyticsService
    {
        public void OnDamaged(EnemyDamagedEvent e) => Debug.Log($"[ANALYTICS] {e}");
        public void OnDied(EnemyDiedEvent e) => Debug.Log($"[ANALYTICS] {e}");
        public void OnReturned(EnemyReturnedToPoolEvent e) => Debug.Log($"[ANALYTICS] {e}");
    }
}
