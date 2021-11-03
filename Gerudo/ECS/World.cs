using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gerudo
{
    public class World
    {
        public string Name { get; private set; }

        private WorldConfig _config;

        private EntityData[] _entities;

        private int _entityCount;

        private int[] _recycledEntities;

        private int _recycledEntityCount;

        private IComponentArray[] _componentArrays;

        private SystemManager _systemManager;

        private Dictionary<EntityMask, EntityFilter> _filters;

        public static int ComponentTypeCount => ComponentTypes.Count;

        public static List<Type> ComponentTypes { get; private set; }

        static World()
        {
            ComponentTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsValueType && typeof(IComponent).IsAssignableFrom(type))
                    {
                        ComponentTypes.Add(type);
                    }
                }
            }
            if (ComponentTypeCount > 64)
            {
                // Current only support 64 types of component
            }
        }

        public World(WorldConfig config)
        {
            _entities = new EntityData[config.defaultEntityCapacity];
            _entityCount = 0;
            _recycledEntities = new int[config.defaultRecycledEntityCapacity];
            _componentArrays = new IComponentArray[ComponentTypeCount];

            _systemManager = new SystemManager();

            _filters = new Dictionary<EntityMask, EntityFilter>();
        }

        public void Initialize()
        {
            _systemManager.Initialize(this);
        }

        public void Update(float deltaTime)
        {
            _systemManager.Update(this, deltaTime);
        }

        public void Destroy()
        {
            _systemManager.Destroy(this);

            for (int i = _entityCount - 1; i >= 0; i--)
            {
                if (_entities[i].version > 0)
                {
                    DestroyEntityInternal(i);
                }
            }
        }

        public Entity CreateEntity()
        {
            Entity entity;
            if (_recycledEntityCount > 0)
            {
                int id = _recycledEntities[--_recycledEntityCount];
                ref var entityData = ref _entities[id];
                Debug.Assert(entityData.version < 0);
                entityData.version = -entityData.version;

                entity.id = id;
                entity.version = entityData.version;
            }
            else
            {
                if (_entityCount == _entities.Length)
                {
                    Array.Resize(ref _entities, _entityCount << 1);
                }

                int id = _entityCount++;
                _entities[id].version = 1;

                entity.id = id;
                entity.version = _entities[id].version;
            }

            return entity;
        }

        public void DestroyEntity(in Entity entity)
        {
#if DEBUG
            if (entity.id < 0 || entity.id >= _entityCount)
            {
                // Log: "Entity {entity.id} has already destroyed!";
            }
#endif
            DestroyEntityInternal(entity.id);
        }

        private void DestroyEntityInternal(int entityId)
        {
            ref var entityData = ref _entities[entityId];

            while (!entityData.componentBitSet.Empty)
            {
                var compId = entityData.componentBitSet.RemoveLastBit();
                var componentArray = _componentArrays[compId];
                componentArray.Delete(entityId);
            }
            UpdateFilter(entityId, entityData.componentBitSet);

            entityData.version = (entityData.version == int.MaxValue) ? -1 : -(entityData.version + 1);

            if (_recycledEntityCount == _recycledEntities.Length)
            {
                Array.Resize(ref _recycledEntities, _recycledEntityCount << 1);
            }

            _recycledEntities[_recycledEntityCount++] = entityId;
        }

        public Entity GetEntity(int entityId)
        {
#if DEBUG
            if (entityId < 0 || entityId >= _entityCount)
            {
                // TODO:
            }
#endif
            ref var entityData = ref _entities[entityId];
            return new Entity { id = entityId, version = entityData.version };
        }

        public bool IsEntityAlive(in Entity entity)
        {
            ref var entityData = ref _entities[entity.id];
            return entity.id >= 0 && entity.id < _entityCount && entityData.version > 0 && entityData.version == entity.version;
        }

        public ref TComp AddComp<TComp>(in Entity entity) where TComp : IComponent
        {
            ref var entityData = ref _entities[entity.id];
#if DEBUG
            if (entityData.componentBitSet.Get<TComp>())
            {
                // TODO: Log error
            }
#endif
            entityData.componentBitSet.Set<TComp>();
            UpdateFilter(entity.id, entityData.componentBitSet);

            var componentArray = GetComponentArray<TComp>();
            return ref componentArray.Add(entity.id);
        }

        public ref TComp GetComp<TComp>(in Entity entity) where TComp : IComponent
        {
            ref var entityData = ref _entities[entity.id];
#if DEBUG
            if (entityData.componentBitSet.Get<TComp>())
            {
                // TODO: Log error
            }
#endif
            var componentArray = GetComponentArray<TComp>();
            return ref componentArray.Get(entity.id);
        }

        public ref TComp GetOrAddComp<TComp>(in Entity entity) where TComp : IComponent
        {
            ref var entityData = ref _entities[entity.id];
            var componentArray = GetComponentArray<TComp>();
            if (!entityData.componentBitSet.Get<TComp>())
            {
                entityData.componentBitSet.Set<TComp>();
                UpdateFilter(entity.id, entityData.componentBitSet);

                return ref componentArray.Add(entity.id);
            }

            return ref componentArray.Get(entity.id);
        }

        public void DeleteComp<TComp>(in Entity entity) where TComp : IComponent
        {
            ref var entityData = ref _entities[entity.id];
            if (entityData.componentBitSet.Get<TComp>())
            {
                entityData.componentBitSet.Reset<TComp>();
                UpdateFilter(entity.id, entityData.componentBitSet);

                var componentArray = GetComponentArray<TComp>();
                componentArray.Delete(entity.id);
            }
        }

        public bool HasComp<TComp>(in Entity entity) where TComp : IComponent
        {
            ref var entityData = ref _entities[entity.id];
            return entityData.componentBitSet.Get<TComp>();
        }

        private void UpdateFilter(int entityId, in ComponentBitSet bitSet)
        {
            foreach (var pair in _filters)
            {
                if (bitSet.Empty || !pair.Key.IsCompatible(bitSet))
                {
                    if (pair.Value.ContainsEntity(entityId))
                    {
                        pair.Value.RemoveEntity(entityId);
                    }
                }
                else
                {
                    if (!pair.Value.ContainsEntity(entityId))
                    {
                        pair.Value.AddEntity(entityId);
                    }
                }
            }
        }

        private ComponentArray<TComp> GetComponentArray<TComp>() where TComp : IComponent
        {
            if (_componentArrays[ComponentType<TComp>.Id] == null)
            {
                _componentArrays[ComponentType<TComp>.Id] = new ComponentArray<TComp>(512);
            }

            return (ComponentArray<TComp>)_componentArrays[ComponentType<TComp>.Id];
        }

        public EntityFilter GetFilter(EntityMask mask)
        {
            if (_filters.TryGetValue(mask, out var filter))
            {
                return filter;
            }

            filter = new EntityFilter(this, mask, 512, _entities.Length);
            for (int i = 0; i < _entityCount; i++)
            {
                ref var entityData = ref _entities[i];
                if (entityData.version > 0 && mask.IsCompatible(entityData.componentBitSet))
                {
                    filter.AddEntity(i);
                }
            }

            _filters.Add(mask, filter);

            return filter;
        }
    }
}