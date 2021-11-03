namespace Gerudo
{
    public class AssetManager
    {
        public bool Startup()
        {
            AssetDatabase.AddLoader(typeof(Model), new ModelLoader());
            AssetDatabase.AddLoader(typeof(Texture2D), new Texture2DLoader());

            return true;
        }

        public void Shutdown()
        {
        }
    }
}