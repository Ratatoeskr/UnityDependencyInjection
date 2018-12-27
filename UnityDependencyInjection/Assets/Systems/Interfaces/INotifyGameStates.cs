namespace com.finalstudio.udi
{
    public interface INotifyGameStates : ISystem
    {
        void FocusLoss();
        void FocusGain();
        void ApplicationQuit();
        void UnPause();
        void Pause();
    }
}