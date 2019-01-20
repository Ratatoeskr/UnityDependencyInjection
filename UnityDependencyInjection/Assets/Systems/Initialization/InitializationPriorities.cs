namespace com.finalstudio.udi.Initialization
{
    public static class InitializationPriorities
    {
        /// <summary>
        ///     Can be used for e.g. downloading resources, preparing files etc.
        /// </summary>
        public const int Preparation = -1000;
        
        /// <summary>
        ///     Can be used for serialization or other stuff that should be done before other systems
        ///     are being initialized
        /// </summary>
        public const int Serialization = -100;
        
        public const int Default = 0;
        
        public const int Late = 100;
        
        public const int Last = 1000;
    }
}