using System;

namespace Gerudo
{
    public struct EcsEntity : IEquatable<EcsEntity>
    {
        public static readonly EcsEntity Null = new EcsEntity();

        internal int Id { get; private set; }

        public bool Equals(EcsEntity other)
        {
            return true;
        }
    }
}