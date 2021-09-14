using System;
using System.Collections.Generic;

namespace Gerudo.ECS
{
    public sealed class World
    {
        internal EntityData[] _entityDatas;

        private int _entitiesCount;

        private int[] _recycledEntities;

        private int _recycledEntitiesCount;

        private IComponentPool[] _componentPools;

        private int _poolsCount;

        private readonly int _poolDenseSize;

        private readonly Dictionary<Type, IComponentPool> _poolHashes;

        private readonly Dictionary<int, ComponentFilter> _hashedFilters;

        private readonly List<ComponentFilter> _allFilters;

        private List<ComponentFilter>[] _filtersByIncludedComponents;

        private List<ComponentFilter>[] _filtersByExcludedComponents;

        private bool _destroyed;

        public World(Config config = null)
        {
            if (config == null)
            {
                config = new Config();
            }

            // entities.
            _entityDatas = new EntityData[config.defaultEntityCount];
            _recycledEntities = new int[config.defaultEntityCount];
            _entitiesCount = 0;
            _recycledEntitiesCount = 0;

            // pools.
            _componentPools = new IComponentPool[config.defaultPoolCount];
            _poolHashes = new Dictionary<Type, IComponentPool> (config.defaultPoolCount);
            _filtersByIncludedComponents = new List<ComponentFilter>[config.defaultPoolCount];
            _filtersByExcludedComponents = new List<ComponentFilter>[config.defaultPoolCount];
            _poolsCount = 0;

            // filters.
            _hashedFilters = new Dictionary<int, ComponentFilter>(config.defaultFilterCount);
            _allFilters = new List<ComponentFilter>(config.defaultFilterCount);
            _poolDenseSize = config.defaultPoolDenseCount;

            _destroyed = false;
        }

        public void Destroy()
        {
            _destroyed = true;
            for (var i = _entitiesCount - 1; i >= 0; i--)
            {
                ref var entityData = ref _entityDatas[i];
                if (entityData.componentCount > 0)
                {
                    DeleteEntity(i);
                }
            }

            _componentPools = null;
            _poolHashes.Clear();
            _hashedFilters.Clear();
            _allFilters.Clear();
            _filtersByIncludedComponents = null;
            _filtersByExcludedComponents = null;
        }

        public bool IsAlive()
        {
            return !_destroyed;
        }

        public Entity CreateEntity(string name = "")
        {
            Entity entity;
            if (_recycledEntitiesCount > 0)
            {
                entity.id = _recycledEntities[--_recycledEntitiesCount];
                ref var entityData = ref _entityDatas[entity.id];
                entityData.version = -entityData.version;
                entityData.name = name;
                entity.version = entityData.version;
            }
            else
            {
                if (_entitiesCount == _entityDatas.Length)
                {
                    // resize entities and component pools.
                    var newSize = _entitiesCount << 1;
                    Array.Resize (ref _entityDatas, newSize);
                    for (int i = 0, iMax = _poolsCount; i < iMax; i++)
                    {
                        _componentPools[i].Resize(newSize);
                    }
                    for (int i = 0, iMax = _allFilters.Count; i < iMax; i++)
                    {
                        _allFilters[i].ResizeSparseIndex(newSize);
                    }
                }
                entity.id = _entitiesCount++;
                ref var entityData = ref _entityDatas[entity.id];
                entityData.version = 1;
                entityData.name = name;
                entity.version = entityData.version;
            }

            entity.world = this;

            return entity;
        }

        public void DeleteEntity(int entityId)
        {
            ref var entityData = ref _entityDatas[entityId];
            if (entityData.version < 0)
            {
                return;
            }

            // kill components.
            if (entityData.componentCount > 0)
            {
                var index = 0;
                while (entityData.componentCount > 0 && index < _poolsCount)
                {
                    for (; index < _poolsCount; index++)
                    {
                        if (_componentPools[index].HasComp(entityId))
                        {
                            _componentPools[index++].DeleteComp(entityId);
                            break;
                        }
                    }
                }
#if DEBUG
                if (entityData.componentCount != 0)
                {
                    throw new Exception ($"Invalid components count on entity {entityId} => {entityData.componentCount}.");
                }
#endif
                return;
            }

            entityData.version = (entityData.version == int.MaxValue ? -1 : -(entityData.version + 1));
            if (_recycledEntitiesCount == _recycledEntities.Length)
            {
                Array.Resize(ref _recycledEntities, _recycledEntitiesCount << 1);
            }
            _recycledEntities[_recycledEntitiesCount++] = entityId;
        }

        public ComponentPool<T> GetPool<T>() where T : struct, IComponent
        {
            var poolType = typeof(ComponentPool<T>);
            if (_poolHashes.TryGetValue(poolType, out var rawPool))
            {
                return (ComponentPool<T>) rawPool;
            }
            var pool = new ComponentPool<T>(this, _poolsCount, _poolDenseSize, _entityDatas.Length);
            _poolHashes[poolType] = pool;
            if (_poolsCount == _componentPools.Length)
            {
                var newSize = _poolsCount << 1;
                Array.Resize (ref _componentPools, newSize);
                Array.Resize (ref _filtersByIncludedComponents, newSize);
                Array.Resize (ref _filtersByExcludedComponents, newSize);
            }
            _componentPools[_poolsCount++] = pool;
            return pool;
        }

        public ComponentMask Filter<T>() where T : struct, IComponent
        {
            return ComponentMask.New(this).Inc<T>();
        }

        internal bool IsEntityAliveInternal(in Entity entity)
        {
            ref var entityData = ref _entityDatas[entity.id];
            return entity.id >= 0 && entity.id < _entitiesCount && entityData.version == entity.version;
        }

        internal (ComponentFilter, bool) GetFilterInternal(ComponentMask mask, int capacity = 512)
        {
            var hash = mask.Hash;
            var exists = _hashedFilters.TryGetValue(hash, out var filter);
            if (exists)
            {
                return (filter, false);
            }

            filter = new ComponentFilter (this, mask, capacity, _entityDatas.Length);
            _hashedFilters[hash] = filter;
            _allFilters.Add(filter);

            // add to component dictionaries for fast compatibility scan.
            for (int i = 0, iMax = mask.IncludeCount; i < iMax; i++)
            {
                var list = _filtersByIncludedComponents[mask.Include[i]];
                if (list == null)
                {
                    list = new List<ComponentFilter>(8);
                    _filtersByIncludedComponents[mask.Include[i]] = list;
                }
                list.Add(filter);
            }
            for (int i = 0, iMax = mask.ExcludeCount; i < iMax; i++)
            {
                var list = _filtersByExcludedComponents[mask.Exclude[i]];
                if (list == null)
                {
                    list = new List<ComponentFilter>(8);
                    _filtersByExcludedComponents[mask.Exclude[i]] = list;
                }
                list.Add(filter);
            }

            // scan exist entities for compatibility with new filter.
            for (int i = 0, iMax = _entitiesCount; i < iMax; i++)
            {
                ref var entityData = ref _entityDatas[i];
                if (entityData.componentCount > 0 && IsMaskCompatible(mask, i))
                {
                    filter.AddEntity(i);
                }
            }

            return (filter, true);
        }

        internal void OnEntityChange(int entity, int componentType, bool added)
        {
            var includeList = _filtersByIncludedComponents[componentType];
            var excludeList = _filtersByExcludedComponents[componentType];
            if (added)
            {
                // add component.
                if (includeList != null)
                {
                    foreach (var filter in includeList)
                    {
                        if (IsMaskCompatible(filter.Mask, entity))
                        {
#if DEBUG
                            if (filter.GetSparseEntities()[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
                            filter.AddEntity(entity);
                        }
                    }
                }
                if (excludeList != null)
                {
                    foreach (var filter in excludeList)
                    {
                        if (IsMaskCompatibleWithout(filter.Mask, entity, componentType))
                        {
#if DEBUG
                            if (filter.GetSparseEntities()[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
                            filter.RemoveEntity(entity);
                        }
                    }
                }
            }
            else
            {
                // remove component.
                if (includeList != null)
                {
                    foreach (var filter in includeList)
                    {
                        if (IsMaskCompatible(filter.Mask, entity))
                        {
#if DEBUG
                            if (filter.GetSparseEntities()[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
                            filter.RemoveEntity(entity);
                        }
                    }
                }
                if (excludeList != null)
                {
                    foreach (var filter in excludeList)
                    {
                        if (IsMaskCompatibleWithout(filter.Mask, entity, componentType))
                        {
#if DEBUG
                            if (filter.GetSparseEntities()[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
                            filter.AddEntity(entity);
                        }
                    }
                }
            }
        }

        bool IsMaskCompatible(ComponentMask filterMask, int entity)
        {
            for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++)
            {
                if (!_componentPools[filterMask.Include[i]].HasComp(entity))
                {
                    return false;
                }
            }
            for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++)
            {
                if (_componentPools[filterMask.Exclude[i]].HasComp(entity))
                {
                    return false;
                }
            }
            return true;
        }

        bool IsMaskCompatibleWithout(ComponentMask filterMask, int entity, int componentId)
        {
            for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++)
            {
                var typeId = filterMask.Include[i];
                if (typeId == componentId || !_componentPools[typeId].HasComp(entity))
                {
                    return false;
                }
            }
            for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++)
            {
                var typeId = filterMask.Exclude[i];
                if (typeId != componentId && _componentPools[typeId].HasComp(entity))
                {
                    return false;
                }
            }
            return true;
        }

        public class Config
        {
            public int defaultEntityCount = 512;
            public int defaultPoolCount = 512;
            public int defaultFilterCount = 512;
            public int defaultPoolDenseCount = 512;
        }

        internal struct EntityData
        {
            public string name;
            public int version;
            public int componentCount;
        }
    }
}