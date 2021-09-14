using System;

namespace Gerudo.ECS
{
    public sealed class ComponentMask
    {
        private World _world;

        internal int[] Include;

        internal int[] Exclude;

        internal int IncludeCount;

        internal int ExcludeCount;

        internal int Hash;

        static readonly object SyncObj = new object ();

        static ComponentMask[] _pool = new ComponentMask[32];

        static int _poolCount;

#if DEBUG
        bool _built;
#endif

        ComponentMask()
        {
            Include = new int[8];
            Exclude = new int[2];
            Reset ();
        }

        void Reset()
        {
            _world = null;
            IncludeCount = 0;
            ExcludeCount = 0;
            Hash = 0;
#if DEBUG
            _built = false;
#endif
        }

        public ComponentMask Inc<T>() where T : struct, IComponent
        {
            var poolId = _world.GetPool<T>().Id;
#if DEBUG
            if (_built) { throw new Exception ("Cant change built mask."); }
            if (Array.IndexOf (Include, poolId, 0, IncludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
            if (Array.IndexOf (Exclude, poolId, 0, ExcludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
#endif
            if (IncludeCount == Include.Length)
            {
                Array.Resize(ref Include, IncludeCount << 1);
            }

            Include[IncludeCount++] = poolId;

            return this;
        }

        public ComponentMask Exc<T>() where T : struct, IComponent
        {
            var poolId = _world.GetPool<T> ().Id;
#if DEBUG
            if (_built) { throw new Exception ("Cant change built mask."); }
            if (Array.IndexOf (Include, poolId, 0, IncludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
            if (Array.IndexOf (Exclude, poolId, 0, ExcludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
#endif
            if (ExcludeCount == Exclude.Length)
            {
                Array.Resize (ref Exclude, ExcludeCount << 1);
            }

            Exclude[ExcludeCount++] = poolId;

            return this;
        }

        public ComponentFilter End(int capacity = 512)
        {
#if DEBUG
            if (_built) { throw new Exception ("Cant change built mask."); }
            _built = true;
#endif
            Array.Sort (Include, 0, IncludeCount);
            Array.Sort (Exclude, 0, ExcludeCount);
            // calculate hash.
            Hash = IncludeCount + ExcludeCount;
            for (int i = 0, iMax = IncludeCount; i < iMax; i++)
            {
                Hash = unchecked (Hash * 314159 + Include[i]);
            }
            for (int i = 0, iMax = ExcludeCount; i < iMax; i++)
            {
                Hash = unchecked (Hash * 314159 - Exclude[i]);
            }

            var (filter, isNew) = _world.GetFilterInternal(this, capacity);
            if (!isNew) { Recycle (); }
            return filter;
        }

        void Recycle()
        {
            Reset();
            lock (SyncObj)
            {
                if (_poolCount == _pool.Length)
                {
                    Array.Resize (ref _pool, _poolCount << 1);
                }
                _pool[_poolCount++] = this;
            }
        }

        internal static ComponentMask New(World world)
        {
            lock (SyncObj)
            {
                var mask = _poolCount > 0 ? _pool[--_poolCount] : new ComponentMask ();
                mask._world = world;
                return mask;
            }
        }
    }
}