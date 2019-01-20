using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace com.finalstudio.udi
{    
    public static class Systems
    {
        private static readonly Dictionary<Type, object> SystemInstances = new Dictionary<Type, object>();
        private static readonly List<Type> SystemTypes = new List<Type>();
        private static readonly List<ISystem> TempRegisteredSystemCache = new List<ISystem>();
        private static readonly List<ISystem> TempUnregisteredSystemCache = new List<ISystem>();

        private static readonly string[] ConfigKeywords = 
        {
            "Config", "config", "_config"
        };
        
        // Cached types for late usage:
        private static Type _typeOfMonoBehaviour;
        private static Type _typeOfListInterface;
        private static Type _typeOfSystemInterface;
        private static Type _typeOfGenericList;
        
        private static readonly Type TypeOfSystemAttribute = typeof(SystemAttribute);
        
        private static GameObject _systemsGameObject;
        private static SystemController _systemController;

        private static SystemConfig[] _systemConfigs;

        private static readonly BindingFlags BindingFlags = 
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly string LogColor = "#d39c50";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void RegisterSystems()
        {
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;

            Debug.Log("Initializing system framework...");
            var watch = new Stopwatch();
                
            // Initialized framework  
            Setup();
            
            GatherSystemTypes();
            GatherSystemConfigs();
            
            Debug.Log("Framework initialization took " + watch.ElapsedMilliseconds + "ms!");
        }

        private static void Setup()
        {
            _systemsGameObject = new GameObject("SYSTEMS");
            _typeOfMonoBehaviour = typeof(MonoBehaviour);
            _typeOfListInterface = typeof(IList);
            _typeOfSystemInterface = typeof(ISystem);
            _typeOfGenericList = typeof(List<>);

            _systemController = _systemsGameObject.AddComponent<SystemController>();            
            
            UnityEngine.Object.DontDestroyOnLoad(_systemsGameObject);            
        }

        private static void GatherSystemConfigs()
        {
            _systemConfigs = Resources.LoadAll<SystemConfig>("Systems");
        }
         
        private static void GatherSystemTypes()
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {                
                foreach (var t in a.GetTypes())
                {
                    var attributes = t.GetCustomAttributes(TypeOfSystemAttribute, false);
                    if (attributes.Length == 0) continue;
                    SystemTypes.Add(t);
                }
            }
        }

        private static void ProcessScene()
        {
            // Get all scene listener from the EXISTING systems. Not the new ones!
            // Otherwise they would  receive the OnSceneLoaded Callback of their own scene 
            var sceneListener = GetSystemArray(typeof(INotifySystemSceneChanged))
                .Cast<INotifySystemSceneChanged>().ToArray();
            var sceneCount = SceneManager.sceneCount;
            var currentScenes = new int[sceneCount];
            for (var i = 0; i < sceneCount; i++)
            {
                currentScenes[i] = SceneManager.GetSceneAt(i).name.GetHashCode();
            }
            
            // Find services of the current scene
            foreach (var systemType in SystemTypes)
            {
                try
                {
                    var attributes = systemType.GetCustomAttributes(TypeOfSystemAttribute, false);
                    var systemAttribute = ((SystemAttribute) attributes[0]);
                    var hashes = systemAttribute.SceneHashes;
                    if (hashes.Any(h => currentScenes.Contains(h)))
                    {
                        if (!SystemInstances.ContainsKey(systemType))
                            RegisterType(systemType, systemAttribute);
                        // if already registered, do nothing!
                    }
                    else
                        UnRegisterType(systemType);                   
                }
                catch (SystemFrameworkException e)
                {
                    // Only catch these kinds of exceptions. Throw the other!
                    Debug.LogError(e);
                }
            }

            // Inject services
            ProcessTypeMembers();

            _systemController.SetSystems(
                GetSystemArray(typeof(INotifySystemUpdate)).Cast<INotifySystemUpdate>().ToArray(),
                GetSystemArray(typeof(INotifyGameStates)).Cast<INotifyGameStates>().ToArray(),
                sceneListener
                );

            CallISystemCallbacks();
        }

        private static void CallISystemCallbacks()
        {
            foreach (var system in TempRegisteredSystemCache)
            {
                try
                {
                    system.Constructor();
                }
                catch (Exception e)
                {
                    LogError(system, e.ToString()); 
                }
            }

            foreach (var system in TempUnregisteredSystemCache)
            {
                system.Destructor();
            }
            
            TempRegisteredSystemCache.Clear();
            TempUnregisteredSystemCache.Clear();
        }

        private static void InjectConfig(object systemInstance, SystemConfig config)
        {
            var systemType = systemInstance.GetType();
            foreach (var configKeyword in ConfigKeywords)
            {
                var member = systemType.GetMember(configKeyword, BindingFlags);
                if (member.Length > 0)
                {
                    member[0].SetUnderlyingType(systemInstance, config);
                    return;
                }
            }
            
            throw new SystemFrameworkException(
                $"System of type {systemType.Name} has no Config-Property but a config is set in the system attribute!");
        }

        private static void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            // Ensure that ObjectLocator really processed scene before systems!
            if (!ObjectLocator.FinishedLocating)
                ObjectLocator.LocateObjects(scene);
            
            ProcessScene();
            _systemController.OnSceneChanged(scene);
        }

        private static void UnRegisterType(Type systemType)
        {
            if (!SystemInstances.ContainsKey(systemType))
                return;

            var systemInstance = SystemInstances[systemType];
            if (_typeOfMonoBehaviour.IsAssignableFrom(systemType))
                UnityEngine.Object.Destroy((MonoBehaviour) systemInstance);

//            (SystemInstances[systemType] as ISystem)?.Destructor();
            if (systemInstance is ISystem system)
                TempUnregisteredSystemCache.Add(system);
            
            SystemInstances.Remove(systemType);
        }
        
        private static void RegisterType(Type systemType, SystemAttribute systemAttribute)
        {
            if (_typeOfMonoBehaviour.IsAssignableFrom(systemType))
            {
                ProcessMonoBehaviour(systemType);
                return;
            }

            var systemInstance = Activator.CreateInstance(systemType);
            SystemInstances.Add(systemType, systemInstance);
            
            if (systemInstance is ISystem system)
                TempRegisteredSystemCache.Add(system);
//            (SystemInstances[systemType] as ISystem)?.Constructor();

            // Inject config:
            if (systemAttribute.Config != null)
            {
                var config =
                    _systemConfigs.FirstOrDefault(c => c.GetType().IsAssignableFrom(systemAttribute.Config));
                if (config == null)
                {
                    throw new SystemFrameworkException(systemType,
                        $"The desired config of type {systemAttribute.Config.Name} " +
                        $"could not be found and therefore not injected!");                    
                }

                InjectConfig(SystemInstances[systemType], config);
            }
        }

        private static void ProcessMonoBehaviour(Type type)
        {
            var instance = ObjectLocator.FindObjectOfType(type)
                ?? _systemsGameObject.AddComponent(type);
            
            SystemInstances.Add(type, instance);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (instance is ISystem system)
                TempRegisteredSystemCache.Add(system);
        }

        private static void ProcessTypeMembers()
        {
            var systemInstances = SystemInstances.Keys;
            foreach (var type in systemInstances)
            {
                var member = type.GetMembers(BindingFlags); 
                for (var i = 0; i < member.Length; i++)
                {
                    if (member[i].MemberType != MemberTypes.Field && member[i].MemberType != MemberTypes.Property)
                        continue;

                    var memberType = member[i].GetUnderlyingType();
                    
                    // Array
                    if (memberType.IsArray)
                    {
                        ProcessArrayMember(member[i], memberType, SystemInstances[type]);
                        continue;
                    } 
                    
                    // List
                    if (_typeOfListInterface.IsAssignableFrom(memberType))
                    {
                        ProcessCollectionMember(member[i], memberType, SystemInstances[type]);
                        continue;
                    }
                    
                    // Interface
                    if (memberType.IsInterface)
                    {
                        // find implementation
                        var implementationType = SystemTypes.Find(t => memberType.IsAssignableFrom(t));
                        if (implementationType == null)
                            continue; // not a system, just an ordinary interface -> skip member
                        
                        // Is implementation a system?
                        var attr = implementationType.GetCustomAttributes(TypeOfSystemAttribute, false);
                        if (attr.Length > 0)
                        {
                            if (SystemInstances.TryGetValue(implementationType, out var implementation))
                                member[i].SetUnderlyingType(SystemInstances[type], implementation);
                            continue;                                
                        }
                    }
                    
                    var attributes =  memberType.GetCustomAttributes(TypeOfSystemAttribute, false);
                    if (attributes.Length == 0) continue;
                    if (!SystemInstances.TryGetValue(memberType, out var memberSystem))
                    {
                        member[i].SetUnderlyingType(SystemInstances[type], null);
                    }
                    
                    member[i].SetUnderlyingType(SystemInstances[type], memberSystem);
                }
            }
        }

        private static void ProcessCollectionMember(MemberInfo memberInfo, Type memberType, object system)
        {
            // Get generic underlying type:
            var typeArgs = memberType.GenericTypeArguments;
            if (typeArgs.Length == 0 || typeArgs.Length > 1) return;
            var elementType = typeArgs[0];
            if (!elementType.IsInterface) return;
            if (!_typeOfSystemInterface.IsAssignableFrom(elementType))
                return;
            if (!_typeOfSystemInterface.IsAssignableFrom(elementType)) return;
            // List contains systems of a specific interface type
            var list = GetByInterfaceType(elementType);
            memberInfo.SetUnderlyingType(system, list);
        }

        private static IList GetByInterfaceType(Type interfaceType)
        {
            var constructedListType = _typeOfGenericList.MakeGenericType(interfaceType);
            var instance = Activator.CreateInstance(constructedListType);
            var list = instance as IList;
            if (list == null) throw new SystemFrameworkException("Could not create list");
            foreach (var system in SystemInstances.Values.Where(interfaceType.IsInstanceOfType))
            {
                list.Add(system);
            }
            return list;
        }

        private static void ProcessArrayMember(MemberInfo memberInfo, Type memberType, object system)
        {
            var elementType = memberType.GetElementType();
            if (elementType == null || !elementType.IsInterface) return;
            
            var array = GetSystemArray(elementType); 
            
            memberInfo.SetUnderlyingType(system, array);
        }

        private static Array GetSystemArray(Type elementType)
        {
            var systems = GetByInterfaceType(elementType);
            var length = systems.Count;
            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
            {
                // Find system
                array.SetValue(systems[i], i);
            }

            return array;
        }

        public static void Log(object system, string msg)
        {            
            Debug.LogFormat($"<color={LogColor}><b>[{system.GetType().Name}]</b> - {Regex.Escape(msg)}</color>");
        }
        
        public static void LogWaring(object system, string msg)
        {
            Debug.LogWarningFormat($"<color={LogColor}><b>[{system.GetType().Name}]</b> - {Regex.Escape(msg)}</color>");
        }
        
        public static void LogError(object system, string msg)
        {
            Debug.LogErrorFormat($"<color={LogColor}><b>[{system.GetType().Name}]</b> - {Regex.Escape(msg)}</color>");
        }

        /// <summary>
        ///     This method has to be called manually because the SceneManager only invokes its callbacks when
        ///     a scene was already successfully loaded
        /// </summary>
        public static void NotifyBeforeSceneChanged()
        {
            var activeScene = SceneManager.GetActiveScene();
            _systemController.OnBeforeSceneChanged(ref activeScene);
        }

        public static T GetSystem<T>()
        {
            var targetType = typeof(T);
            foreach (var keyValuePair in SystemInstances)
            {
                if (targetType.IsAssignableFrom(keyValuePair.Key))
                    return (T) keyValuePair.Value;
                if (keyValuePair.Key == targetType)
                    return (T) keyValuePair.Value;
            }

            return default;
        }
    }
}