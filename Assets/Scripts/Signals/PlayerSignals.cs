namespace Scripts
{
    public struct PlayerDiedSignal
    {
        public IPlayerModel Model;
    }

    public struct PlayerEnemyCollidedSignal
    {
        public EnemyView Enemy;
        public int Damage;
    }
}
