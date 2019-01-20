using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.finalstudio.udi.Initialization
{
    /// <summary>
    ///     Create your own implementation and define the scenes in which you need initialization
    ///     via the system-attribute
    /// </summary>
    public abstract class InitializationSystem : IInitializationSystem
    {
        public virtual string CurrentInitializationMessage { get; protected set; }
        public virtual float CurrentProgress { get; protected set; }

        // ReSharper disable once UnassignedField.Global
        protected IInitializable[] Initializables;
        // ReSharper disable once UnassignedField.Global
        protected IInitializationListener[] InitializationListeners;

        public virtual void StartInitialization()
        {
            // Are there initializable systems?
            if (Initializables == null || Initializables.Length == 0) return;
            var sorted = Initializables.Where(i => !i.IsInitialized).OrderBy(i => i.Priority);
            CoroutineHelper.ExecuteCoroutine(InitializeRoutine(sorted));
        }

        protected virtual IEnumerator InitializeRoutine(IEnumerable<IInitializable> initializables)
        {
            var systems = initializables as IInitializable[] ?? initializables.ToArray();
            var totalCount = (float) systems.Length;
            var i = 0;
            var progressPerSystem = 1 / totalCount;
            foreach (var system in systems)
            {                 
                var initialization = system.Initialization();
                var @continue = true; 
                while (@continue) 
                {
                    CurrentProgress = i /  totalCount 
                                      + system.InitializationProgress * progressPerSystem;
                    CurrentInitializationMessage = system.Message;
                    @continue = initialization.MoveNext();
                    yield return initialization;
                } 

                system.IsInitialized = true;

                i++;
            }
            
            // Everything initialized:
            EverythingInitialized(systems);
        }

        protected virtual void EverythingInitialized(IInitializable[] systems)
        {
            // IInitializeables are also listener and therefore also set by framework as IInitializeListener
            foreach (var listener in InitializationListeners)
            {
                listener.InitializationFinished();
            }
        }
    }

    public interface IInitializationSystem
    {
        void StartInitialization();
    }
}