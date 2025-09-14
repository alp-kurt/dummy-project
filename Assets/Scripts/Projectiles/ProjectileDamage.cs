namespace Scripts
{
    public sealed class ProjectileDamage : IUpgradable
    {
        public int Value { get; private set; }

        public ProjectileDamage(int baseDamage)
        {
            Value = baseDamage;
        }

        public void Upgrade(int amount)
        {
            Value += amount;
        }
    }
}
