namespace Gerudo
{
    public interface ISubModule
    {
        void Startup();

        void Update();

        void Shutdown();
    }
}