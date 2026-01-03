# Mobile Vampire Survivors Prototype

This repository contains a Unity 2D prototype inspired by *Vampire Survivors*. It explores how to structure a maintainable mobile-scale project using Zenject-powered dependency injection, MVP boundaries, and reactive programming patterns across core gameplay systems.

## Repository Map

The project is split into feature-focused folders, each documenting their responsibilities:

- [Assets/README.md](Assets/README.md) — high-level overview of the gameplay pillars, setup instructions, and architectural background.
- [Assets/Scripts/Global/FactoryBase/README.md](Assets/Scripts/Global/FactoryBase/README.md) — shared object factory and pooling foundation used by every runtime system.
- [Assets/Scripts/Player/README.md](Assets/Scripts/Player/README.md) — player MVP implementation, health management, and collision handling.
- [Assets/Scripts/EnemySystem/README.md](Assets/Scripts/EnemySystem/README.md) — enemy models, presenters, views, and wave spawning.
- [Assets/Scripts/ProjectileSystem/README.md](Assets/Scripts/ProjectileSystem/README.md) — projectile abstractions and presenter pipeline.
- [Assets/Scripts/ProjectileSystem/Bolt/README.md](Assets/Scripts/ProjectileSystem/Bolt/README.md) — concrete bolt projectile behaviour and configuration.
- [Assets/Scripts/HUD/EnemyKillCounter/README.md](Assets/Scripts/HUD/EnemyKillCounter/README.md) — reactive HUD counter driven by global enemy death streams.

## Getting Started

Open the project with Unity 6000.3.2f1 LTS or newer. For detailed setup (layers, installers, ScriptableObject configs) refer to the main [Assets/README](Assets/README.md).

## Branches

The `fully-reactive` branch (work in progress) refactors the original MVP + pooling hybrid toward a simpler, purely reactive architecture. Use it to compare design approaches or continue iterating on the reactive rewrite.

## License

This prototype is shared for educational purposes. Review individual assets and scripts before using them in production.
