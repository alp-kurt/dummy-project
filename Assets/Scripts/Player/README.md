# Player System

A lightweight Player implementation. Uses MVP (Model–View–Presenter), Zenject for DI, UniRx for reactive streams, and DOTween for light UI polish.

## Features

* Reactive Health System

    * PlayerHealthModel: Damaged, Healed, Died streams + CurrentHealth01 (0..1)

    * PlayerHealthPresenter: binds normalized health to a slider with a tween

* Layer-aware collisions

    * Player only reacts to Enemy layer; fast TryGetComponent path, parent fallback

## Script Structure

Scripts/Players
* IPlayerModel.cs
* PlayerModel.cs
* PlayerView.cs
* PlayerPresenter.cs
* PlayerInstaller.cs

Scripts/Players/PlayerHealth
* IPlayerHealthModel.cs
* PlayerHealthModel.cs
* PlayerHealthView.cs
* PlayerHealthPresenter.cs

## Public API (Core Types)

* IPlayerModel

        Vector2 MoveInput { get; }
        bool IsWalking { get; }
        void SetMoveInput(Vector2 input);
        Vector3 Step(float deltaTime); // returns frame delta movement in world space

* IPlayerHealthModel

        IReadOnlyReactiveProperty<float> CurrentHealth01 { get; } // 0..1
        float MaxHealth { get; }
        IObservable<Unit> Died { get; }
        IObservable<int> Damaged { get; }
        IObservable<int> Healed { get; }

        void ReceiveDamage(int amountHp);
        void Heal(int amountHp);
        void ResetFull(); // revive to full (e.g., rewarded ad)

* PlayerView

        Vector2 Position { get; }
        event Action<EnemyView> OnEnemyCollided;
        void Translate(Vector3 delta);

## Setup & Usage

### Physics Layers

1. Create/ensure layers: Player, Enemy, Projectile.

2. In Project Settings → Physics 2D → Layer Collision Matrix: enable Player ↔ Enemy; disable irrelevant pairs.

3. In PlayerView, confirm the enemyLayerName (default "Enemy").

### Enemy Prefabs

1. Put enemy colliders on Enemy layer.

2. Place EnemyView on the same GameObject as the collider (preferred). Parent lookup is supported as fallback

### Scene Wiring

1. Add PlayerInstaller to the scene and assign:

2. JoystickView (emits IObservable<Vector2> OnInput)

3. PlayerView (on Player root with collider set to Player layer)

4. PlayerHealthView (slider UI)

5. Set Player Max Health and Player Move Speed in PlayerInstaller.