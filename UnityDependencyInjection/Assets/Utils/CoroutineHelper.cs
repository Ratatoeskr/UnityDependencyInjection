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

        public static void AwaitOperation(AsyncOperation operation, Action callback)
        {
            Instance.StartCoroutine(Instance.AwaitOperationRoutine(operation, callback));
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

        private IEnumerator AwaitOperationRoutine(AsyncOperation operation, Action callback)
        {
            yield return operation;
            // Just to be sure that operation is really done yet:
            while (!operation.isDone) yield return null;
            callback?.Invoke();
        }
    }
}