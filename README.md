# UnityDependencyInjection
Dependency Injection via Systems for Unity

An attempt to use dependency injection in unity via so-called systems. This "framework" is an very early stage, but feel free to give feedback or to contribute :)
Readme is Work-In-Progress!

# Prerequisites
This framework is based on latest Unity 2018.3 features such as
* Package Management
* RuntimeInitializeOnLoadMethod
* C# 7.0 / .Net 4.6
* Test Runner

# Differences to existing Frameworks
There are some great and big frameworks like Zenject which already offers dependency injection for Unity3D.  
This framework aims to provide next to dependency injection also  a solid design pattern for a fast and streamlined software design solution for Unity3D games. 

# How to use
Create a new system class. It can be a MonoBehaviour or not, doesn't matter. Important: a system requires the system-attribute!
```cs
// This can be recognized as system (you don't need implementing the interface):
[System(Scenes=new []{"MyScene"})]
public class MySystem : ISystem
{
   public void Constructor() {}
   public void Detructor() {}
}

// This can also be recognized as a system:
[System(Scenes=new []{"MyScene"})]
public class MyMonoBehaviourSystem : MonoBehaviour
{
  // this field is automatically being set by the framework
  private MySystem _mySystem;
}
```

You don't have to use constructor injection. This is a current design decision as systems can also be MonoBehaviours which are created without constructors. On the left hand this is being a great and lazy solution as you don't have to mark or further implement injection constructs. On the other hand this leads to compile warnings (649) and code inspection issues. So maybe there will be a attribute later to compensate the named issue.

## System-Attribute
**Scenes** defines in which scenes this system exists. In case of MonoBehaviour the system can already be placed in the scene, but doesn't have to. It will be automatically instantiated otherwise.

# Use your own serialized properties via configs
Use your own Config-Classes to expose properties to the inspector. This works also for Non-Monobehaviour types
```cs
[CreateAssetMenu(fileName = "MySystemConfig", menuName = "Config/MySystemConfig")]
public class MySystemConfig : SystemConfig
{
  // automatically set by framework
  private MySystemConfig _config;
}
```

Unfortunatly configs **must be placed in the resources folder** for now! This may change in the future!

# Arrays
You can access other systems by their interface abstractions:
```cs
[System(Scenes = new[] {"MyScene", "AnotherScene"}, Config = typeof(SerializationConfig))]
public class SerializationSystem : ISerializationSystem
{
  // automatically set by framework
  public SerializationConfig Config { get; set; }
  // All systems which implements ISerializationSystem. Automatically set by framework!
  private ISerializationSystem[] _serializeables;
}
```

# Use as package via PackageManager
Clone this repository. Then edit your `manifest.json` in your own Unity Project and edit the relative path accordingly
```json
{
  "dependencies": {
    ...
    "com.finalstudio.udi" : "file:../../../../UDI/UnityDependencyInjection/Assets"
  }
}
```

