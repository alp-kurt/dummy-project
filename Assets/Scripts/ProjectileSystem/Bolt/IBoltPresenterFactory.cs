using UnityEngine;

namespace Scripts
{
    public interface IBoltPresenterFactory
    {
        BoltPresenter Create(BoltView view, ProjectileConfig config);
    }
}