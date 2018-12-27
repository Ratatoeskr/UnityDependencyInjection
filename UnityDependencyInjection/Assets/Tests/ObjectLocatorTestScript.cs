using System.Collections;
using com.finalstudio.udi;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class ObjectLocatorTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ObjectLocatorTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestFindingRootComponent()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
            yield return null;
            Assert.NotNull(ObjectLocator.FindObjectByType<BoxCollider>());           
        }
        
        [UnityTest]
        public IEnumerator TestFindNestedComponents()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
            yield return null;
            Assert.AreEqual(ObjectLocator.FindObjectsByType<SphereCollider>().Length, 3);           
        }
    }
}
