using System;
using System.Collections.Generic;

namespace Gerudo
{
    public class Model : IAsset, IDisposable
    {
        public List<Mesh> meshes = new List<Mesh>();

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