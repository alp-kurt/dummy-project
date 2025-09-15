namespace Scripts
{
    public interface IBoltSpawnerModel
    {
        /// <summary>Configured seconds between casts.</summary>
        float IntervalSeconds { get; }

        /// <summary>Configured count per cast.</summary>
        int BoltsPerCast { get; }

        /// <summary>Accumulate delta time; returns true when a cast should trigger this frame.</summary>
        bool Tick(float deltaTime);

        /// <summary>Reset accumulator after a cast triggers.</summary>
        void ConsumeTrigger();
    }
}
