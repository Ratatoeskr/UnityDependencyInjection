using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace com.finalstudio.udi
{
    public static class ObjectLocator
    {
        public static int SearchDepth = 3;
        internal static bool FinishedLocating { get; private set; }
        private static int _lastProcessedScene = -1;

        /// <summary>
        ///     Item1 = BuildIndex
        ///     Item2 = Recursively extracted MonoBehaviours
        /// </summary>
        private static readonly List<Tuple<int, List<Component>>> Hierarchy 
            = new List<Tuple<int, List<Component>>>();
        
        private static readonly List<Component> CachedComponents = new List<Component>();

        private static readonly Type TypeOfMonoBehaviour = typeof(Component);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitializeOnLoadMethod()
        {
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            
            SceneManager.sceneUnloaded -= SceneManagerOnSceneUnloaded;
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
            
            SceneManagerOnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void SceneManagerOnSceneUnloaded(Scene scene)
        {                        
            var tuple = Hierarchy.Find(t => t.Item1 == scene.buildIndex);
            if (tuple == null) return;
            tuple.Item2.Clear();
            Hierarchy.Remove(tuple);
        }

        private static void CollectChildren(Transform transform, int depth, ref List<Component> list)
        {            
            list.AddRange(transform.GetComponents(TypeOfMonoBehaviour));
            depth++;
            if (depth >= SearchDepth) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                CollectChildren(transform.GetChild(i), depth, ref list);
            }
        }

        private static void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (FinishedLocating) return;
            LocateObjects(scene);
        }

        internal static void LocateObjects(Scene scene)
        {
            var watch = new Stopwatch();
            if (scene.buildIndex == _lastProcessedScene) return;
            
            CachedComponents.Clear();
            
            var tuple = Hierarchy.Find(t => t.Item1 == scene.buildIndex);
            if (tuple == null)
            {
                tuple = new Tuple<int, List<Component>>(scene.buildIndex, new List<Component>());
                Hierarchy.Add(tuple);
            }
            else
            {
                tuple.Item2.Clear();
            }

            var rootObjects = scene.GetRootGameObjects();
            var list = tuple.Item2;
            for (var i = 0; i < rootObjects.Length; i++)
            {
                CollectChildren(rootObjects[i].transform, 0, ref list);
            }
            
            Hierarchy.ForEach(t => CachedComponents.AddRange(t.Item2));
            FinishedLocating = true;
            CoroutineHelper.ExecuteAfterOneFrame(() => FinishedLocating = false);
            _lastProcessedScene = scene.buildIndex;
            
            Debug.Log("Locating objects took " + watch.ElapsedMilliseconds + "ms!");
        }

        public static T FindObjectByType<T>() where T : Component
        {
            return (T) CachedComponents.FirstOrDefault(c => c is T);
        } 
        
        public static T[] FindObjectsByType<T>() where T : Component
        {
            return CachedComponents.Where(c => c is T).Cast<T>().ToArray();
        }

        public static object FindObjectOfType(Type type)
        {
            return CachedComponents.FirstOrDefault(c =>
            {
                var toc = c.GetType(); 
                return toc.IsSubclassOf(type) || toc == type;
            });
        }
    }
}