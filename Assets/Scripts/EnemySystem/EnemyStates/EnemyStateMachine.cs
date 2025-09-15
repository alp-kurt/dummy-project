namespace Scripts
{
    public sealed class EnemyStateMachine
    {
        private readonly EnemyContext _ctx;
        private IEnemyState _current;

        public IEnemyState Current => _current;
        public string CurrentName => _current?.Name ?? "None";

        public EnemyStateMachine(EnemyContext ctx)
        {
            _ctx = ctx;
        }

        public void Start(IEnemyState startState)
        {
            _current = startState;
            _current?.OnEnter(_ctx);
        }

        public void Update(float deltaTime)
        {
            _current?.OnUpdate(_ctx, deltaTime);
        }

        public void Transition(IEnemyState next)
        {
            if (next == null || next == _current) return;
            _current?.OnExit(_ctx);
            _current = next;
            _current?.OnEnter(_ctx);
        }
    }
}
