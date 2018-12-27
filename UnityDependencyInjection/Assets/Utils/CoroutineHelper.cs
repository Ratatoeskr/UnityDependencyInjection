using System;
using System.Collections;
using UnityEngine;

namespace com.finalstudio.udi
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        
        private static CoroutineHelper Instance => _instance != null 
            ? _instance 
            : _instance = (new GameObject("CoroutineHelper").AddComponent<CoroutineHelper>());

        public static void ExecuteAfterOneFrame(Action callback)
        {
            Instance.StartCoroutine(Instance.WaitOneFrame(callback));
        }
        
        public static void ExecuteAfterSeconds(float seconds, Action callback)
        {
            Instance.StartCoroutine(Instance.WaitForSeconds(seconds, callback));
        }

        private IEnumerator WaitForSeconds(float time, Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }

        private IEnumerator WaitOneFrame(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }
    }
}