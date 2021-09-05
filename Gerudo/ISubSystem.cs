namespace Gerudo
{
    public interface ISubSystem
    {
        void Startup();

        void Update();

        void Shutdown();
    }
}