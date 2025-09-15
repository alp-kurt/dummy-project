# Kill Counter HUD

A tiny, reusable system that tracks enemy kills and displays the count on the HUD.

## Features

* Reactive counter 
        
        (IReadOnlyReactiveProperty<int> Count)

* Decoupled UI via IEnemyKillCounterView

* Inspector-tunable label format ("Kills: {0}")

## How It Works

1. EnemyKillCounterPresenter.Initialize() subscribes to IEnemyDeathStream.Died and calls model.Increment().

2. Presenter also subscribes to model.Count and calls view.SetCount(value).

3. On teardown, Dispose() cleans up subscriptions.

## Setup

1. Place EnemyKillCounterView on a HUD GameObject with a TextMeshProUGUI child.

2. Add an Installer and bind:

        Container.Bind<EnemyKillCounterView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IEnemyKillCounterModel>().To<EnemyKillCounterModel>().AsSingle();
        Container.BindInterfacesTo<EnemyKillCounterPresenter>().AsSingle().NonLazy();

3. Ensure your enemy system publishes deaths through IEnemyDeathStream.