using com.finalstudio.udi;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Tests.TestData.Systems
{
    [System(Scenes = new []{"TestScene"})]
    public class TestSystemA : ISystem
    {
        public static TestSystemA TestInstance { get; private set; }

#pragma warning disable 649
        private TestSystemB _systemB;
#pragma warning restore 649

        public bool HasSystemB()
        {
            return _systemB != null;
        }
        
        public void Constructor()
        {            
            TestInstance = this;
            TestSystemB.TestInstance = _systemB;            
        }

        public void Destructor()
        {
        }
    }
    
    [System(Scenes = new []{"TestScene"}, Config = typeof(TestConfig))]
    public class TestSystemB
    {
        public static TestSystemB TestInstance { get; set; }
        
        private TestConfig _config;
        
        private TestSystemA _testSystemA;

        public bool HasConfig()
        {
            return _config != null;
        }
    }

    [System(Scenes = new []{"TestScene"})]
    public class SceneChangedListenerA : INotifySystemSceneChanged
    {
        public void Constructor()
        {
        }

        public void Destructor()
        {
        }

        public void OnSceneChanged(Scene activeScene)
        {
            Debug.Log("OnSceneChanged was called!");
        }

        public void OnBeforeSceneChanged(Scene activeScene)
        {
        }
    }
}