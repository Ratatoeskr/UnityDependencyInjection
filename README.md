# UnityDependencyInjection
Dependency Injection via Systems for Unity

An attempt to use dependency injection in unity via so-called systems. This "framework" is an very early stage, but feel free to give feedback or to contribute :)
Readme is Work-In-Progress!

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
