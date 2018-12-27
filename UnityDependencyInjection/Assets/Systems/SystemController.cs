using UnityEngine;

namespace com.finalstudio.udi
{
    public class SystemController : MonoBehaviour
    {
        private INotifySystemUpdate[] _updatedSystems;
        private INotifyGameStates[] _notifyStateSystems;
        
        public void SetSystems(
            INotifySystemUpdate[] updatedSystems,
            INotifyGameStates[] stateSystems)
        {
            _updatedSystems = updatedSystems;
            _notifyStateSystems = stateSystems;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            for (var i = 0; i < _notifyStateSystems.Length; i++)
            {
                if (hasFocus) _notifyStateSystems[i].FocusGain();
                else _notifyStateSystems[i].FocusLoss();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            for (var i = 0; i < _notifyStateSystems.Length; i++)
            {
                if (pauseStatus) _notifyStateSystems[i].Pause();
                else _notifyStateSystems[i].UnPause();
            }
        }

        private void OnApplicationQuit()
        {
            for (var i = 0; i < _notifyStateSystems.Length; i++)
            {
                _notifyStateSystems[i].ApplicationQuit();
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            for (var i = 0; i < _updatedSystems.Length; i++)
            {
                _updatedSystems[i].UpdateSystem(deltaTime);
            }
        }
    }
}