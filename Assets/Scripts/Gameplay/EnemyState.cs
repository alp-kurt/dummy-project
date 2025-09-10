
namespace Scripts
{
    public enum EnemyState
    {
        Inactive,   // pooled / not spawned yet
        OnScreen,   // visible / active
        OffScreen   // spawned but currently out of camera view
    }
}
