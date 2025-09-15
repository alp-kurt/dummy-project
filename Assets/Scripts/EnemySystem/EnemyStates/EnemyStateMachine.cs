namespace Scripts
{
    public sealed class EnemyStateMachine
    {
        private readonly EnemyContext m_ctx;
        private IEnemyState m_current;

        public IEnemyState Current => m_current;
        public string CurrentName => m_current?.Name ?? "None";

        public EnemyStateMachine(EnemyContext ctx)
        {
            m_ctx = ctx;
        }

        public void Start(IEnemyState startState)
        {
            m_current = startState;
            m_current?.OnEnter(m_ctx);
        }

        public void Update(float deltaTime)
        {
            m_current?.OnUpdate(m_ctx, deltaTime);
        }

        public void Transition(IEnemyState next)
        {
            if (next == null || next == m_current) return;
            m_current?.OnExit(m_ctx);
            m_current = next;
            m_current?.OnEnter(m_ctx);
        }
    }
}
