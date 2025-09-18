using UniRx;
using UnityEngine;

namespace Scripts
{
    public interface IProjectileModel : IDamager
    {
        string Name { get; }
        Sprite Sprite { get; }
        float Speed { get; } 
        int Damage { get; } 

        bool IsActive { get; }
        IReadOnlyReactiveProperty<bool> IsActiveRx { get; }

        void Activate();
        void Deactivate();

        void SetStats(int damage, float speed);
    }
}
