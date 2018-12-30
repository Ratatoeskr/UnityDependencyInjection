namespace com.finalstudio.udi
{
    public interface ISystem
    {
        /// <summary>
        ///     Called after creation of all systems in the current scene.
        ///     That means that all system members are set
        /// </summary>
        void Constructor();
        
        /// <summary>
        ///     Called after all old systems which does no longer exists are deleted
        /// </summary>
        void Destructor();
    }
}