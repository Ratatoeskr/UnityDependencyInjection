using System.Collections;
using System.Collections.Generic;
using com.finalstudio.udi;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestSceneListenerScript
    {
        [UnityTest]
        public IEnumerator TestOnSceneChangedIsCalled()
        {
            LogAssert.Expect(LogType.Log, "OnSceneChanged was called!");
            yield return null;
            // Test if first scene also notified scene change!
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
            yield return null;                                   
        }
    }
}
