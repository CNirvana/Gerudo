using System;
using System.Collections.Generic;

namespace Gerudo
{
    public class Model : IAsset, IDisposable
    {
        public List<Mesh> meshes = new List<Mesh>();

        public List<Animation> animations = new List<Animation>();

        public void Dispose()
        {
            foreach (var mesh in meshes)
            {
                mesh.Dispose();
            }
            meshes.Clear();
        }
    }
}