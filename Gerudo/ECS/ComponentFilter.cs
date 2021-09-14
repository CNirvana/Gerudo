using System;

namespace Gerudo.ECS
{
    public sealed class ComponentFilter
    {
        public World World { get; private set; }

        internal ComponentMask Mask { get; private set; }

        private int[] _denseEntities;

        private int _entitiesCount;

        private int[] _sparseEntities;

        private int _lockCount;

        private DelayedOp[] _delayedOps;

        private int _delayedOpsCount;

        internal ComponentFilter(World world, ComponentMask mask, int denseCapacity, int sparseCapacity)
        {
            World = world;
            Mask = mask;

            _denseEntities = new int[denseCapacity];
            _sparseEntities = new int[sparseCapacity];
            _entitiesCount = 0;
            _delayedOps = new DelayedOp[512];
            _delayedOpsCount = 0;
            _lockCount = 0;
        }

        public Enumerator GetEnumerator()
        {
            _lockCount++;
            return new Enumerator(this);
        }

        internal int[] GetSparseEntities()
        {
            return _sparseEntities;
        }

        internal void ResizeSparseIndex(int capacity)
        {
            Array.Resize (ref _sparseEntities, capacity);
        }

        internal void AddEntity(int entity)
        {
            if (AddDelayedOp(true, entity))
            {
                return;
            }

            if (_entitiesCount == _denseEntities.Length)
            {
                Array.Resize (ref _denseEntities, _entitiesCount << 1);
            }

            _denseEntities[_entitiesCount++] = entity;
            _sparseEntities[entity] = _entitiesCount;
        }

        internal void RemoveEntity(int entity)
        {
            if (AddDelayedOp(false, entity))
            {
                return;
            }

            var idx = _sparseEntities[entity] - 1;
            _sparseEntities[entity] = 0;
            _entitiesCount--;

            if (idx < _entitiesCount)
            {
                _denseEntities[idx] = _denseEntities[_entitiesCount];
                _sparseEntities[_denseEntities[idx]] = idx + 1;
            }
        }

        bool AddDelayedOp(bool added, int entity)
        {
            if (_lockCount <= 0)
            {
                return false;
            }

            if (_delayedOpsCount == _delayedOps.Length)
            {
                Array.Resize (ref _delayedOps, _delayedOpsCount << 1);
            }

            ref var op = ref _delayedOps[_delayedOpsCount++];
            op.Added = added;
            op.Entity = entity;
            return true;
        }

        void Unlock()
        {
#if DEBUG
            if (_lockCount <= 0) {
                throw new Exception ($"Invalid lock-unlock balance for \"{GetType ().Name}\".");
            }
#endif
            _lockCount--;
            if (_lockCount == 0 && _delayedOpsCount > 0)
            {
                for (int i = 0, iMax = _delayedOpsCount; i < iMax; i++)
                {
                    ref var op = ref _delayedOps[i];
                    if (op.Added)
                    {
                        AddEntity (op.Entity);
                    }
                    else
                    {
                        RemoveEntity (op.Entity);
                    }
                }
                _delayedOpsCount = 0;
            }
        }

        public struct Enumerator : IDisposable
        {
            readonly ComponentFilter _filter;
            readonly int[] _entities;
            readonly int _count;
            int _idx;

            public Enumerator(ComponentFilter filter)
            {
                _filter = filter;
                _entities = filter._denseEntities;
                _count = filter._entitiesCount;
                _idx = -1;
            }

            public int Current
            {
                get => _entities[_idx];
            }

            public bool MoveNext()
            {
                return ++_idx < _count;
            }

            public void Dispose()
            {
                _filter.Unlock();
            }
        }

        struct DelayedOp
        {
            public bool Added;
            public int Entity;
        }
    }
}