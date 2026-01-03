# Player System

A lightweight Player implementation. Uses MVP (Model–View–Presenter), Zenject for DI, UniRx for reactive streams, and DOTween for light UI polish.

## Features

* Reactive health owned by `PlayerModel`

    * `PlayerModel` exposes `CurrentHealth`/`MaxHealth`, raises `Died`, and handles damage movement input.

    * PlayerHealthPresenter: binds normalized health to a slider with a tween

* Layer-aware collisions

    * `PlayerView` only reacts to the Enemy layer; fast `TryGetComponent` path with parent fallback.

* Input-driven movement

    * `JoystickView` publishes joystick vectors; `PlayerPresenter` forwards them to the model and applies the resulting step to the view each frame.

## Script Structure

Scripts/Player

* `IPlayerModel.cs`
* `PlayerModel.cs`
* `PlayerView.cs`
* `PlayerPresenter.cs`
* `PlayerInstaller.cs`
* `PlayerHealthBarView.cs`
* `JoyStick/JoystickView.cs`

## Public API (Core Types)

* `IPlayerModel`

        Vector2 MoveInput { get; }
        bool IsWalking { get; }
        void SetMoveInput(Vector2 input);
        Vector3 Step(float deltaTime); // returns frame delta movement in world space
        float MaxHealth { get; }
        IObservable<Unit> Died { get; }

* `PlayerView`

        Vector2 Position { get; }
        event Action<EnemyView> OnEnemyCollided;
        void Translate(Vector3 delta);

* `PlayerHealthBarView`

        // MonoBehaviour (injects IPlayerModel)
        // Automatically finds Slider/Image children if left unassigned.
        // Subscribes to IPlayerModel.CurrentHealth and updates UI to 0..1.

* `JoystickView`

        IObservable<Vector2> OnInput { get; }
        // Emits normalized drag deltas while pressed. Resets to zero on release/pause.

* Signals

        PlayerDiedSignal
        PlayerEnemyCollidedSignal
## Setup & Usage

### Physics Layers

1. Create/ensure layers: Player, Enemy, Projectile.

2. 2. In **Project Settings → Physics 2D → Layer Collision Matrix**: enable Player ↔ Enemy; disable irrelevant pairs.

3. In `PlayerView`, confirm the `enemyMask` includes the Enemy layer (default "Enemy").

### Enemy Prefabs

1. Put enemy colliders on the Enemy layer.

2. Place `EnemyView` on the same GameObject as the collider (preferred). Parent lookup is supported as fallback.


### Scene Wiring

1. Drop a `PlayerInstaller` into the scene.
2. Assign the scene `JoystickView` instance in the installer (required).
3. Ensure your Player GameObject has a `PlayerView` (Zenject resolves it via `FromComponentInHierarchy`).
4. Optionally add `PlayerHealthBarView` to a UI object containing a `Slider`/`Image`; it auto-binds when instantiated with Zenject.
5. Configure **Player Max Health** and **Player Move Speed** on the installer as needed.
6. Signals are declared in `PlayerInstaller`; they are optional subscribers by default.
