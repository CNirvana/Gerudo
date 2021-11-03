namespace Gerudo
{
    public interface ISubModule
    {
        bool Startup();

        void Update(float deltaTime);

        void Shutdown();
    }
}