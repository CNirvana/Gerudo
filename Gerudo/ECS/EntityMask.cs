using System;

namespace Gerudo
{
    public class EntityMask : IEquatable<EntityMask>
    {
        private ComponentBitSet _include;

        private ComponentBitSet _exclude;

        public EntityMask Include<TComp>() where TComp : IComponent
        {
            _include.Set<TComp>();
            return this;
        }

        public EntityMask Exclude<TComp>() where TComp : IComponent
        {
            _exclude.Set<TComp>();
            return this;
        }

        public bool IsCompatible(in ComponentBitSet bitSet)
        {
            return _include.Include(bitSet) && _exclude.Exclude(bitSet);
        }

        public bool Equals(EntityMask other)
        {
            return _include == other._include && _exclude == other._exclude;
        }

        public override bool Equals(object obj)
        {
            if (obj is EntityMask mask)
            {
                return _include == mask._include && _exclude == mask._exclude;
            }

            return false;
        }

        // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode/263416#263416
        public override int GetHashCode()
        {
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ _include.GetHashCode();
                hashcode = hashcode * 7302013 ^ _exclude.GetHashCode();
                return hashcode;
            }
        }

        public static bool operator == (in EntityMask left, in EntityMask right)
        {
            return left._include == right._include && left._exclude == right._exclude;
        }

        public static bool operator != (in EntityMask left, in EntityMask right)
        {
            return !(left == right);
        }
    }
}