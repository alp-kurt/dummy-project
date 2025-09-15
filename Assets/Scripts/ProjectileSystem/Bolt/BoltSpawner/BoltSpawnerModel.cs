namespace Scripts
{
    public sealed class BoltSpawnerModel : IBoltSpawnerModel
    {
        private readonly BoltSpawnerConfig _cfg;
        private float _accumulator;
        private bool _ready;

        public BoltSpawnerModel(BoltSpawnerConfig cfg)
        {
            _cfg = cfg;
        }

        // Read live from config so designers can tweak at runtime.
        public float IntervalSeconds => _cfg ? _cfg.secondsBetweenCasts : 0.5f;
        public int BoltsPerCast => _cfg ? _cfg.boltsPerCast : 1;

        public bool Tick(float deltaTime)
        {
            var interval = IntervalSeconds > 0f ? IntervalSeconds : 0.05f;
            _accumulator += deltaTime;
            if (_accumulator >= interval) _ready = true;
            return _ready;
        }

        public void ConsumeTrigger()
        {
            if (!_ready) return;

            var interval = IntervalSeconds > 0f ? IntervalSeconds : 0.05f;
            _accumulator -= interval;           // keep leftover time to avoid drift
            if (_accumulator < 0f) _accumulator = 0f;
            _ready = false;
        }
    }
}
