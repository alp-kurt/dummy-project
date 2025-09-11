namespace Scripts
{
    public enum EnemyPooledReason { Unknown = 0, Offscreen = 1, Dead = 2 }

    public readonly struct EnemyDamagedEvent
    {
        public readonly int Amount;
        public readonly int NewHealth;
        public EnemyDamagedEvent(int amount, int newHealth)
        { Amount = amount; NewHealth = newHealth; }
        public override string ToString() => $"Damaged: {Amount}, NewHealth: {NewHealth}";
    }

    public readonly struct EnemyDiedEvent
    {
        public readonly int SpawnId;
        public EnemyDiedEvent(int spawnId) { SpawnId = spawnId; }
        public override string ToString() => $"Died (SpawnId={SpawnId})";
    }

    public readonly struct EnemyReturnedToPoolEvent
    {
        public readonly int SpawnId;
        public readonly EnemyPooledReason Reason;
        public EnemyReturnedToPoolEvent(int spawnId, EnemyPooledReason reason)
        { SpawnId = spawnId; Reason = reason; }
        public override string ToString() => $"ReturnedToPool (SpawnId={SpawnId}, Reason={Reason})";
    }
}
