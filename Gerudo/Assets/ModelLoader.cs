using System.Numerics;

namespace Gerudo
{
    public class ModelLoader : IAssetLoader
    {
        public IAsset Load(string path)
        {
            var importer = new Assimp.AssimpContext();
            var importFlags = Assimp.PostProcessSteps.Triangulate
                | Assimp.PostProcessSteps.GenerateNormals
                | Assimp.PostProcessSteps.FlipWindingOrder
                | Assimp.PostProcessSteps.FlipUVs;
            var scene = importer.ImportFile(path, importFlags);

            if (scene.HasMeshes == false)
            {
                return null;
            }

            var model = new Model();
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = ReadMesh(scene.Meshes[i]);
                model.meshes.Add(mesh);
            }

            return model;
        }

        private Mesh ReadMesh(Assimp.Mesh meshData)
        {
            var meshDataUVs = meshData.HasTextureCoords(0) ? meshData.TextureCoordinateChannels[0] : null;
            var defaultUV = new Assimp.Vector3D(0, 0, 0);
            var meshDataColors = meshData.HasVertexColors(0) ? meshData.VertexColorChannels[0] : null;
            var defaultColor = new Assimp.Color4D(1, 1, 1, 1);

            Vertex[] vertices = new Vertex[meshData.VertexCount];
            for (int i = 0; i < meshData.VertexCount; i++)
            {
                ref var vertex = ref vertices[i];

                var position = meshData.Vertices[i];
                vertex.position = new Vector3(position.X, position.Y, position.Z);

                var texcoord = meshDataUVs != null ? meshDataUVs[i] : defaultUV;
                vertex.texcoord = new Vector2(texcoord.X, texcoord.Y);

                var color = meshDataColors != null ? meshDataColors[i] : defaultColor;
                vertex.color = new Veldrid.RgbaFloat(color.R, color.G, color.B, color.A);

                var normal = meshData.Normals[i];
                vertex.normal = new Vector3(normal.X, normal.Y, normal.Z);
            }

            var shortIndices = meshData.GetShortIndices();
            ushort[] indices = new ushort[shortIndices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort)shortIndices[i];
            }

            return Mesh.Create(vertices, indices);
        }
    }
}