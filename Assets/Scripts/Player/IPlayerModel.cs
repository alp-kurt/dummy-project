using UniRx;

namespace Scripts
{
    public interface IPlayerModel
    {
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }
    }
}