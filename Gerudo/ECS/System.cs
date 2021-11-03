namespace Gerudo
{
    public interface ISystem
    {
    }

    public interface IInitSystem : ISystem
    {
        void Init(World world);
    }

    public interface IUpdateSystem : ISystem
    {
        void Update(World world, float deltaTime);
    }

    public interface IDestroySystem : ISystem
    {
        void Destroy(World world);
    }
}