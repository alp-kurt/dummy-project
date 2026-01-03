using UnityEngine;
using Zenject;

namespace Scripts
{
    public interface IBoltTargetingService
    {
        Vector3 GetRedirectDirection(Vector3 fromPosition);
    }

    public sealed class BoltTargetingService : IBoltTargetingService
    {
        private readonly IEnemyRegistry _enemyRegistry;

        public BoltTargetingService(IEnemyRegistry enemyRegistry)
        {
            _enemyRegistry = enemyRegistry;
        }

        public Vector3 GetRedirectDirection(Vector3 fromPosition)
        {
            var target = _enemyRegistry.FindClosestVisible(fromPosition);
            if (target != null)
                return NormalizeOrRandom(target.Position - fromPosition);

            return SafeRandomDirection();
        }

        private static Vector3 NormalizeOrRandom(Vector3 direction)
        {
            return direction.sqrMagnitude > 0f ? direction.normalized : SafeRandomDirection();
        }

        private static Vector3 SafeRandomDirection()
        {
            var rnd = UnityEngine.Random.insideUnitCircle;
            if (rnd == Vector2.zero) rnd = Vector2.right;
            rnd.Normalize();
            return new Vector3(rnd.x, rnd.y, 0f);
        }
    }
}
