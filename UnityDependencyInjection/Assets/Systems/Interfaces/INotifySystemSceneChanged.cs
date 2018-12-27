using UnityEngine.SceneManagement;

namespace com.finalstudio.udi
{
    public interface INotifySystemSceneChanged : ISystem
    {
        void OnSceneChanged(Scene activeScene);
        void OnBeforeSceneChanged(Scene activeScene);
    }
}