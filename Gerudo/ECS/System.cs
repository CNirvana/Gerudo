namespace Gerudo.ECS
{
    public interface IEcsSystem { }

    public interface IPreInitSystem : IEcsSystem
    {
        void PreInit(SystemManager systems);
    }

    public interface IInitSystem : IEcsSystem
    {
        void Init(SystemManager systems);
    }

    public interface IUpdateSystem : IEcsSystem
    {
        void Update(SystemManager systems);
    }

    public interface IDestroySystem : IEcsSystem
    {
        void Destroy(SystemManager systems);
    }

    public interface IPostDestroySystem : IEcsSystem
    {
        void PostDestroy(SystemManager systems);
    }
}