using System;
using System.Collections;
using System.Collections.Generic;

namespace Gerudo
{
    public class EntityFilter : IEnumerable<Entity>
    {
        private World _world;

        private EntityMask _mask;

        private int[] _entities;

        private int _entityCount;

        private int[] _sparseEntities;

        private DelayOp[] _delayOps;

        private int _delayOpCount;

        private int _lockCount;

        public EntityFilter(World world, EntityMask mask, int capacity, int entityCount)
        {
            _world = world;
            _mask = mask;
            _entities = new int[capacity];
            _entityCount = 0;
            _sparseEntities = new int[entityCount];

            _delayOps = new DelayOp[128];
            _delayOpCount = 0;

            _lockCount = 0;
        }

        public void AddEntity(int entityId)
        {
            if (PushDelayOp(entityId, added: true))
            {
                return;
            }

            if (_entityCount == _entities.Length)
            {
                Array.Resize(ref _entities, _entityCount << 1);
            }

            _entities[_entityCount++] = entityId;
            _sparseEntities[entityId] = _entityCount;
        }

        public void RemoveEntity(int entityId)
        {
            if (PushDelayOp(entityId, added: false))
            {
                return;
            }

            var index = _sparseEntities[entityId] - 1;
            _sparseEntities[entityId] = 0;
            _entityCount--;
            if (index < _entityCount)
            {
                _entities[index] = _entities[_entityCount];
                _sparseEntities[_entities[index]] = index + 1;
            }
        }

        public bool ContainsEntity(int entityId)
        {
            return _sparseEntities[entityId] > 0;
        }

        private bool PushDelayOp(int entityId, bool added)
        {
            if (_lockCount > 0)
            {
                _delayOps[_delayOpCount].entityId = entityId;
                _delayOps[_delayOpCount++].added = added;
                return true;
            }

            return false;
        }

        private void Lock()
        {
            _lockCount++;
        }

        private void Unlock()
        {
            _lockCount--;
            if (_lockCount == 0 && _delayOpCount > 0)
            {
                for (int i = 0; i < _delayOpCount; i++)
                {
                    ref var op = ref _delayOps[i];
                    if (op.added)
                    {
                        AddEntity(op.entityId);
                    }
                    else
                    {
                        RemoveEntity(op.entityId);
                    }
                }
            }
        }

        public struct Enumerator : IEnumerator<Entity>
        {
            public Entity Current => _filter._world.GetEntity(_filter._entities[_index]);

            object IEnumerator.Current => _filter._world.GetEntity(_filter._entities[_index]);

            private EntityFilter _filter;

            private int _index;

            public Enumerator(EntityFilter filter)
            {
                _filter = filter;
                _index = -1;
                _filter.Lock();
            }

            public bool MoveNext()
            {
                return ++_index < _filter._entityCount;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
                _filter.Unlock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return new Enumerator(this);
        }

        struct DelayOp
        {
            public bool added;

            public int entityId;
        }
    }
}