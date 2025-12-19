public interface IStaminaObserver
{
    void OnStaminaChanged(float current, float max);
    void OnStaminaEmpty();
}