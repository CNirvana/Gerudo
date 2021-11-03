using System;

namespace Gerudo
{
    public struct ComponentBitSet : IEquatable<ComponentBitSet>
    {
        public bool Empty => _bitSet == 0;

        private ulong _bitSet;

        public void Set<TComp>() where TComp : IComponent
        {
            int id = ComponentType<TComp>.Id;
            _bitSet |= 1UL << id;
        }

        public void Reset<TComp>() where TComp : IComponent
        {
            int id = ComponentType<TComp>.Id;
            _bitSet &= ~(1UL << id);
        }

        public bool Get<TComp>() where TComp : IComponent
        {
            int id = ComponentType<TComp>.Id;
            return (_bitSet & (1UL << id)) != 0;
        }

        public bool Include(in ComponentBitSet other)
        {
            return (_bitSet & other._bitSet) == other._bitSet;
        }

        public bool Exclude(in ComponentBitSet other)
        {
            return (_bitSet & other._bitSet) == 0;
        }

        public int RemoveLastBit()
        {
            for (int i = 0; i < 64; i++)
            {
                if (((1UL << i) & _bitSet) != 0)
                {
                    _bitSet = (_bitSet - 1) & _bitSet;
                    return i;
                }
            }

            return -1;
        }

        public override int GetHashCode()
        {
            return _bitSet.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ComponentBitSet bitSet)
            {
                return _bitSet == bitSet._bitSet;
            }

            return false;
        }

        public bool Equals(ComponentBitSet other)
        {
            return _bitSet == other._bitSet;
        }

        public static bool operator == (in ComponentBitSet left, in ComponentBitSet right)
        {
            return left._bitSet == right._bitSet;
        }

        public static bool operator != (in ComponentBitSet left, in ComponentBitSet right)
        {
            return !(left == right);
        }
    }
}