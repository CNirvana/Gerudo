using Veldrid;

namespace Gerudo
{
    public class Material
    {
        public Texture2D MainTexture { get; private set; }

        internal ResourceLayout Layout { get; private set; }

        internal ResourceSet MainTextureSet { get; private set; }

        public Material(Texture2D mainTexture)
        {
            this.MainTexture = mainTexture;

            var device = Engine.Instance.RenderSystem.Device;

            this.Layout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            this.MainTextureSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                Layout, mainTexture.View, mainTexture.Sampler));
        }
    }
}