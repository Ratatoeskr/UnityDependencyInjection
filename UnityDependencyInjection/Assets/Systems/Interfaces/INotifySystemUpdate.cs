namespace com.finalstudio.udi
{
    public interface INotifySystemUpdate : ISystem
    {
        void UpdateSystem(float deltaTime);
    }
}