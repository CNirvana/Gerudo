namespace Gerudo
{
    public struct Entity
    {
        internal int id;

        internal int version;
    }

    public static class EntityExtensions
    {
        public static TComp GetComp<TComp>(in Entity entity) where TComp : IComponent
        {
            return default;
        }
    }
}