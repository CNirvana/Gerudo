namespace Gerudo
{
    internal static class ComponentId
    {
        internal static int Id { get; set; } = 0;
    }

    public static class ComponentType<TComp> where TComp : IComponent
    {
        public static int Id { get; private set; }

        static ComponentType()
        {
            Id = ComponentId.Id++;
        }
    }
}