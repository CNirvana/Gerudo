using System;
using System.Collections.Generic;

namespace Gerudo
{
    public class SystemManager
    {
        private List<ISystem> _allSystems = new List<ISystem>();

        private List<IInitSystem> _initSystems = new List<IInitSystem>();

        private List<IUpdateSystem> _updateSystems = new List<IUpdateSystem>();

        private List<IDestroySystem> _destroySystems = new List<IDestroySystem>();

        public SystemManager()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsInterface && typeof(ISystem).IsAssignableFrom(type))
                    {
                        _allSystems.Add((ISystem)Activator.CreateInstance(type));

                        if (typeof(IInitSystem).IsAssignableFrom(type))
                        {
                            _initSystems.Add((IInitSystem)Activator.CreateInstance(type));
                        }

                        if (typeof(IUpdateSystem).IsAssignableFrom(type))
                        {
                            _updateSystems.Add((IUpdateSystem)Activator.CreateInstance(type));
                        }

                        if (typeof(IDestroySystem).IsAssignableFrom(type))
                        {
                            _destroySystems.Add((IDestroySystem)Activator.CreateInstance(type));
                        }
                    }
                }
            }
        }

        public void Initialize(World world)
        {
            foreach (var system in _initSystems)
            {
                system.Init(world);
            }
        }

        public void Update(World world, float deltaTime)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(world, deltaTime);
            }
        }

        public void Destroy(World world)
        {
            foreach (var system in _destroySystems)
            {
                system.Destroy(world);
            }
        }
    }
}