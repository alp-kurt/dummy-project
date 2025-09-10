using UniRx;

namespace Scripts
{
    public class PlayerModel : IPlayerModel
    {
        private readonly ReactiveProperty<float> _currentHealth = new(1);
        
        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
    }
}