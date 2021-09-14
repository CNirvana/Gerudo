using System;

namespace Gerudo.ECS
{
    public interface IComponent
    {
    }

    public interface IComponentPool
    {
        int Id { get; }

        Type ComponentType { get; }

        void Resize(int capacity);

        bool HasComp(int entityId);

        void DeleteComp(int entityId);

        IComponent GetRawComp(int entityId);
    }

    public interface IComponentAutoReset<TComp> where TComp : struct, IComponent
    {
        void AutoReset(ref TComp c);
    }

    public sealed class ComponentPool<TComp> : IComponentPool where TComp : struct, IComponent
    {
        delegate void AutoResetHandler (ref TComp component);

        public Type ComponentType { get; private set; }

        public World World { get; private set; }

        public int Id { get; private set; }

        private readonly AutoResetHandler _autoReset;

        private TComp[] _denseItems;

        private int[] _sparseItems;

        private int _denseItemsCount;

        private int[] _recycledItems;

        private int _recycledItemsCount;

        internal ComponentPool(World world, int id, int denseCapacity, int sparseCapacity)
        {
            ComponentType = typeof(TComp);
            World = world;
            Id = id;

            _denseItems = new TComp[denseCapacity + 1];
            _sparseItems = new int[sparseCapacity];
            _denseItemsCount = 1;
            _recycledItems = new int[512];
            _recycledItemsCount = 0;
            var isAutoReset = typeof(IComponentAutoReset<TComp>).IsAssignableFrom(ComponentType);

#if DEBUG
            if (!isAutoReset && ComponentType.GetInterface ("IComponentAutoReset`1") != null)
            {
                throw new Exception($"IComponentAutoReset should have <{typeof(TComp).Name}> constraint for component \"{typeof(TComp).Name}\".");
            }
#endif

            if (isAutoReset)
            {
                var autoResetMethod = typeof(TComp).GetMethod(nameof(IComponentAutoReset<TComp>.AutoReset));
#if DEBUG
                if (autoResetMethod == null)
                {
                    throw new Exception (
                        $"IComponentAutoReset<{typeof(TComp).Name}> explicit implementation not supported, use implicit instead.");
                }
#endif
                _autoReset = (AutoResetHandler)Delegate.CreateDelegate(
                    typeof(AutoResetHandler),
                    null,
                    autoResetMethod);
            }
        }

        void IComponentPool.Resize(int capacity)
        {
            Array.Resize(ref _sparseItems, capacity);
        }

        IComponent IComponentPool.GetRawComp(int entity)
        {
            return _denseItems[_sparseItems[entity]];
        }

        public ref TComp AddComp(int entityId) {
#if DEBUG
            if (_sparseItems[entityId] > 0)
            {
                throw new Exception ("Already attached.");
            }
#endif
            int index;
            if (_recycledItemsCount > 0)
            {
                index = _recycledItems[--_recycledItemsCount];
            }
            else
            {
                index = _denseItemsCount;
                if (_denseItemsCount == _denseItems.Length)
                {
                    Array.Resize(ref _denseItems, _denseItemsCount << 1);
                }
                _denseItemsCount++;
                _autoReset?.Invoke (ref _denseItems[index]);
            }

            _sparseItems[entityId] = index;
            World.OnEntityChange(entityId, Id, true);
            World._entityDatas[entityId].componentCount++;

            return ref _denseItems[index];
        }

        public ref TComp GetComp(int entityId)
        {
#if DEBUG
            if (_sparseItems[entityId] == 0)
            {
                throw new Exception ("Not attached.");
            }
#endif
            return ref _denseItems[_sparseItems[entityId]];
        }

        public bool HasComp(int entity)
        {
            return _sparseItems[entity] > 0;
        }

        public void DeleteComp(int entityId)
        {
            ref var sparseData = ref _sparseItems[entityId];
            if (sparseData > 0)
            {
                World.OnEntityChange (entityId, Id, false);
                if (_recycledItemsCount == _recycledItems.Length)
                {
                    Array.Resize (ref _recycledItems, _recycledItemsCount << 1);
                }
                _recycledItems[_recycledItemsCount++] = sparseData;
                if (_autoReset != null)
                {
                    _autoReset.Invoke (ref _denseItems[sparseData]);
                }
                else
                {
                    _denseItems[sparseData] = default;
                }
                sparseData = 0;
                ref var entityData = ref World._entityDatas[entityId];
                entityData.componentCount--;

                if (entityData.componentCount == 0)
                {
                    World.DeleteEntity(entityId);
                }
            }
        }
    }
}