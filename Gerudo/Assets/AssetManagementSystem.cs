namespace Gerudo
{
    public class AssetManagementSystem : ISubSystem
    {
        public void Startup()
        {
            AssetManager.AddLoader(typeof(Model), new ModelLoader());
            AssetManager.AddLoader(typeof(Texture2D), new Texture2DLoader());
        }

        public void Update()
        {
        }

        public void Shutdown()
        {
        }
    }
}