using System;

namespace com.finalstudio.udi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SystemAttribute : Attribute
    {
        private string[] _scenes;

        public string[] Scenes
        {
            get => _scenes;
            set
            {
                _scenes = value; 
                CalcHashes();
            }
        }

        public Type Config { get; set; }
        
        internal int[] SceneHashes { get; private set; }

        private void CalcHashes()
        {
            SceneHashes = new int[Scenes.Length];
            for (int i = 0; i < Scenes.Length; i++)
            {
                SceneHashes[i] = Scenes[i].GetHashCode();
            }
        }
    }
}