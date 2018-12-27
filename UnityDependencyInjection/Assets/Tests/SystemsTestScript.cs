using System.Collections;
using Tests.TestData.Systems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests
{
    public class SystemsTestScript
    {
        [UnityTest]
        public IEnumerator TestIfSystemsAreRegistered()
        {
            yield return null;
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
            yield return null;
            
            // Now test whether system were registered
            Assert.IsNotNull(TestSystemA.TestInstance);
            Assert.IsTrue(TestSystemA.TestInstance.HasSystemB());
        }
        
        [UnityTest]
        public IEnumerator TestIfSystemConfigIsSet()
        {
            yield return null;
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
            yield return null;
            Assert.IsTrue(TestSystemB.TestInstance.HasConfig());
        }
    }
}
