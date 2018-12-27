using System;

namespace com.finalstudio.udi
{
    public class SystemFrameworkException : Exception
    {
        public SystemFrameworkException(Type systemType, string message = "Error occured!") 
            : base($"System Framework - Error on {systemType.Name}: {message}")
        {
        }
        
        public SystemFrameworkException(string message = "Error occured!") 
            : base($"System Framework: {message}")
        {
        }
    }
}