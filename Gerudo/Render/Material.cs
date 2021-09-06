using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Gerudo
{
    public class Material
    {
        public Texture2D MainTexture { get; private set; }

        internal Pipeline Pipeline { get; private set; }

        internal ResourceSet ResourceSet { get; private set; }

        public Material(Texture2D mainTexture)
        {
            this.MainTexture = mainTexture;

            var device = Engine.Instance.RenderSystem.Device;
            var factory = device.ResourceFactory;

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(Shaders.COLOR_VERT),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(Shaders.COLOR_FRAG),
                "main");

            var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            this.ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                layout, mainTexture.View, mainTexture.Sampler));

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { Vertex.Layout },
                shaders: shaders);
            pipelineDescription.ResourceLayouts = new[] {
                GlobalBuffers.PerFrameBuffer.Layout,
                GlobalBuffers.PerDrawBuffer.Layout,
                layout };
            pipelineDescription.Outputs = new OutputDescription(
                new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
                new [] { new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm) }
            );

            Pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        // public MaterialInstance CreateInstance()
        // {
        // }
    }
}