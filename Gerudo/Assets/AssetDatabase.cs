using System;
using System.Collections.Generic;

namespace Gerudo
{
    public static class AssetDatabase
    {
        private static Dictionary<Type, IAssetLoader> _loaders = new Dictionary<Type, IAssetLoader>();

        public static T LoadAsset<T>(string path) where T : IAsset
        {
            if (_loaders.TryGetValue(typeof(T), out var loader))
            {
                return (T)loader.Load(path);
            }

            return default(T);
        }

        internal static void AddLoader(Type type, IAssetLoader loader)
        {
            _loaders.Add(type, loader);
        }
    }
}