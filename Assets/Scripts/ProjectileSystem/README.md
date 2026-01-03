# Projectile System Foundation

This module defines the core projectile architecture. It provides a reusable base for all projectile types (e.g., bolts, boomerang) in the game. It is designed for MVP separation, pooling support, and extensibility.

## Features

* Data-driven config via ProjectileConfigBase. Designers can use the extended config files from this base. They can set name, sprite, base damage, base speed.
* Stat wrappers (ProjectileDamage, ProjectileSpeed) with upgrade methods, allowing projectiles to scale dynamically at runtime.
* Model layer (ProjectileModel):
    * Holds projectile state (damage, speed, name, sprite).
    * Reactive active flag (IsActiveRx) to drive view changes.
    * Provides Activate() / Deactivate() lifecycle hooks.
* View layer (ProjectileView):
    * Sprite rendering.
    * Collider + LayerMask filtering.
    * Publishes collision hits as IObservable<IDamageable>.
    * Fires ProjectileHitSignal via SignalBus (optional subscribers).
    * Pooling-ready (OnRent / OnRelease).
* Presenter layer (ProjectilePresenter):
    * Connects model and view.
    * Handles motion each frame (EveryUpdate).
    * Routes hits from view to damage targets.
    * Exposes DespawnRequested signal for pooling handles/factories.
    * Fires ProjectileDespawnedSignal on despawn (optional subscribers).
    * Extensible via OnSpawned() and AttachToSpawn() for subclasses.
* Interfaces:
    * IProjectileModel abstracts projectile state, speed, damage, and lifecycle.
    * Inherits IDamager for integration with combat systems.

## Design Decisions

* MVP split (Model–View–Presenter)
    * Keeps state (model), visuals/collision (view), and orchestration (presenter) separate → testable, extensible.
* Reactive patterns (UniRx)
    * Active state and collisions are exposed as observables. This avoids polling and enables easy subscription by other systems.
* Pooling-aware
    * View derives from PooledView, presenter rebuilds per-spawn disposables in InitializeMotion(), ensuring safe reuse.
* Despawn signaling
    * Projectiles don’t return themselves to the pool. Instead, they raise DespawnRequested, letting the pool/handle coordinate recycling.
* Composable upgrades
    * Damage/Speed wrapped in dedicated classes with Upgrade() to support perk/buff systems later.


## API Overview

* ProjectileConfigBase

        public abstract class ProjectileConfigBase : ScriptableObject {
        string DisplayName;
        Sprite Sprite;
        int BaseDamage;
        float BaseSpeed;
        }

* ProjectileModel

        public abstract class ProjectileModel : IProjectileModel {
        string Name;
        Sprite Sprite;
        int Damage;
        float Speed;
        IReadOnlyReactiveProperty<bool> IsActiveRx;

        void Activate();
        void Deactivate();
        void SetDamage(ProjectileDamage dmg);
        void SetSpeed(ProjectileSpeed spd);
        }

* ProjectileView

        public class ProjectileView : PooledView {
        IObservable<IDamageable> HitTargets;
        void SetSprite(Sprite sprite);
        void SetActive(bool active);
        void SetPosition(Vector3 pos);
        void Move(Vector3 delta);
        }

* ProjectilePresenter

        public class ProjectilePresenter : IInitializable, IDisposable {
        ProjectileView View;
        IObservable<Unit> DespawnRequested;

        void Initialize(); // one-time
        void InitializeMotion(Vector3 pos, Vector3 dir); // per-spawn
        void PrepareForDespawn(); // before pooling
        }

## Trade-offs

* Per-projectile EveryUpdate() is simple but can become costly at scale (>100 projectiles). Can be replaced with a centralized mover system later.

* Direct Debug.Log removed: avoids spam in hot paths. Add optional logging via a debug flag if needed.

* Stat setters (SetDamage, SetSpeed) allow flexibility but can lead to inconsistent state if misused. A more robust modifier pipeline could be introduced.

## ProjectileConfigBase (Shared Projectile Settings)

Base settings shared by all projectile types (name, sprite, base stats). Specific projectile types (e.g., BoltConfig) inherit this and add their own fields.

* Create: You don’t create this directly; use a concrete projectile asset like BoltConfig below.

* Used where: In the system that fires projectiles (e.g., player weapon / projectile installer). Ask your programmer which scene object exposes a …Config field for your projectile.

### Fields (Designer Guide)

* Display Name
    * Friendly name for UI/debugging.

* Sprite
    * Visual for the projectile. The asset warns if this is missing.

* Base Damage
    * Damage per hit before runtime modifiers (perks, buffs). Minimum 

* Base Speed
    * Units per second. 0 means it won’t move (the asset warns).

The asset trims empty names, clamps damage/speed, and warns on missing sprite or zero speed.
