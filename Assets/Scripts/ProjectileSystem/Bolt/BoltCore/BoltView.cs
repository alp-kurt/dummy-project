using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Visual for the Bolt projectile.
    /// Inherits pooled behavior and shared projectile utilities from ProjectileView.
    /// </summary>
    public sealed class BoltView : ProjectileView
    {
        // Intentionally empty. Add Bolt-only visual bits later if needed.
        // Pooling lifecycle comes from PooledView via ProjectileView.
        // CachedTransform, SetSprite, SetActive etc. are inherited.
    }
}
