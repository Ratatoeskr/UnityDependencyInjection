using System.Collections;

namespace com.finalstudio.udi.Initialization
{
    public interface IInitializable : IInitializationListener
    {
        /// <summary>
        ///     This property defines the order in which initializables are initialized. The higher the
        ///     priority the later the system is initialized compared to other initializable systems.
        ///
        ///     You can make use of the static class InitializationPriorities
        ///     <see cref="InitializationPriorities"/>
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Do everything in this method what you want to initialize.
        ///     Automatically called the the initialization system during the initialization progress
        /// </summary>
        /// <returns></returns>
        IEnumerator Initialization();

        /// <summary>
        ///     Optional: Initialization progress for this specific system
        /// </summary>
        float InitializationProgress { get; }

        /// <summary>
        ///     Optional: Message which can be e.g. displayed during a loading screen
        /// </summary>
        string Message { get; }       
        
        bool IsInitialized { get; set; }
    }
}