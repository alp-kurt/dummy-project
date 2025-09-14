using UnityEngine;

namespace Scripts
{
    public sealed class BoltModel : ProjectileModel, IBoltModel
    {
        public BoltModel(string name, Sprite sprite, ProjectileDamage damage, ProjectileSpeed speed)
            : base(name, sprite, damage, speed) { }
    }
}
