namespace Scripts
{
    public interface IHealable
    {
        void Heal(int amount);
        void ResetFull();
    }
}