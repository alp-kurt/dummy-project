namespace Scripts
{
    public sealed class ProjectileSpeed : IUpgradable
    {
        public float Value { get; private set; }

        public ProjectileSpeed(float baseSpeed)
        {
            Value = baseSpeed;
        }

        // Interface requirement; integer steps still supported.
        public void Upgrade(int amount)
        {
            Value += amount;
        }

        // Convenience for float steps.
        public void Upgrade(float amount)
        {
            Value += amount;
        }
    }
}
