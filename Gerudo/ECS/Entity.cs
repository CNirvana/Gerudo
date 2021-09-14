using System;

namespace Gerudo.ECS
{
    public struct Entity : IEquatable<Entity>
    {
        internal int id;

        internal int version;

        internal World world;

        public bool Equals(Entity other)
        {
            return id == other.id && version == other.version && world == other.world;
        }
    }

    public static class EntityExtensions
    {
        public static bool IsAlive(this Entity entity)
        {
            return entity.world.IsEntityAliveInternal(entity);
        }

        public static void Delete(this Entity entity)
        {
#if DEBUG
            if (!entity.IsAlive())
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            entity.world.DeleteEntity(entity.id);
        }

        public static ref TComp AddComp<TComp>(this Entity entity) where TComp : struct, IComponent
        {
#if DEBUG
            if (!entity.IsAlive())
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            var pool = entity.world.GetPool<TComp>();
            return ref pool.AddComp(entity.id);
        }

        public static ref TComp GetComp<TComp>(this Entity entity) where TComp : struct, IComponent
        {
#if DEBUG
            if (!entity.IsAlive())
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            var pool = entity.world.GetPool<TComp>();
            return ref pool.GetComp(entity.id);
        }

        public static bool HasComp<TComp>(this Entity entity) where TComp : struct, IComponent
        {
#if DEBUG
            if (!entity.IsAlive())
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            var pool = entity.world.GetPool<TComp>();
            return pool.HasComp(entity.id);
        }
    }
}