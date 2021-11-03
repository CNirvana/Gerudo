using System;

namespace Gerudo
{
    public interface IComponentArray
    {
        void Delete(int entityId);
    }

    public sealed class ComponentArray<TComp> : IComponentArray where TComp : IComponent
    {
        private TComp[] _comps;

        private int _compCount;

        private int[] _sparseComps;

        private int[] _recycledComps;

        private int _recycledCount;

        public ComponentArray(int capacity)
        {
            _comps = new TComp[capacity];
            _compCount = 0;
            _sparseComps = new int[capacity];
            _recycledComps = new int[512];
            _recycledCount = 0;
        }

        public ref TComp Add(int entityId)
        {
            int index = 0;
            if (_recycledCount > 0)
            {
                index = _recycledComps[--_recycledCount];
            }
            else
            {
                if (_compCount == _comps.Length)
                {
                    Array.Resize(ref _comps, _compCount << 1);
                }
                index = _compCount++;
            }

            _sparseComps[entityId] = index;
            return ref _comps[index];
        }

        public ref TComp Get(int entityId)
        {
            return ref _comps[_sparseComps[entityId]];
        }

        public void Delete(int entityId)
        {
            if (_recycledCount == _recycledComps.Length)
            {
                Array.Resize(ref _recycledComps, _recycledCount << 1);
            }

            // TODO: auto reset
            _sparseComps[entityId] = default;
            _recycledComps[_recycledCount++] = _sparseComps[entityId];
        }
    }
}