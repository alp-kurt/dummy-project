namespace Scripts
{
    public interface IPooledViewModule
    {
        void OnSpawn();    // reset initial state for a fresh lifetime
        void OnDespawn();  // kill tweens, clear visuals, etc.
    }
}
