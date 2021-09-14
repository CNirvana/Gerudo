using System.Collections.Generic;

namespace Gerudo.ECS
{
    public sealed class SystemManager
    {
        private readonly World _defaultWorld;

        private readonly Dictionary<string, World> _worlds;

        private readonly List<IEcsSystem> _allSystems;

        private readonly List<IUpdateSystem> _updateSystems;

        public SystemManager(World defaultWorld)
        {
            _defaultWorld = defaultWorld;
            _worlds = new Dictionary<string, World>(32);
            _allSystems = new List<IEcsSystem>(128);
            _updateSystems = new List<IUpdateSystem>(128);
        }

        public World GetWorld (string name = null)
        {
            if (name == null)
            {
                return _defaultWorld;
            }

            _worlds.TryGetValue (name, out var world);

            return world;
        }

        public void Destroy()
        {
            for (var i = _allSystems.Count - 1; i >= 0; i--)
            {
                if (_allSystems[i] is IDestroySystem destroySystem)
                {
                    destroySystem.Destroy (this);
                }
            }

            for (var i = _allSystems.Count - 1; i >= 0; i--)
            {
                if (_allSystems[i] is IPostDestroySystem postDestroySystem)
                {
                    postDestroySystem.PostDestroy (this);
                }
            }

            _allSystems.Clear ();
            _updateSystems.Clear();
        }

        public SystemManager AddWorld(World world, string name)
        {
            _worlds[name] = world;
            return this;
        }

        public SystemManager Add(IEcsSystem system)
        {
            _allSystems.Add(system);
            if (system is IUpdateSystem updateSystem)
            {
                _updateSystems.Add(updateSystem);
            }
            return this;
        }

        public void Init()
        {
            foreach (var system in _allSystems)
            {
                if (system is IPreInitSystem initSystem)
                {
                    initSystem.PreInit (this);
                }
            }

            var runIdx = 0;
            foreach (var system in _allSystems)
            {
                if (system is IInitSystem initSystem)
                {
                    initSystem.Init (this);
                }

                if (system is IUpdateSystem runSystem)
                {
                    _updateSystems[runIdx++] = runSystem;
                }
            }
        }

        public void Update()
        {
            foreach (var updateSystem in _updateSystems)
            {
                updateSystem.Update(this);
            }
        }
    }
}