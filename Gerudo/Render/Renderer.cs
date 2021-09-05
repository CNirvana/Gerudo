using System;

namespace Gerudo
{
    public class Renderer : IDisposable
    {
        public Transform Transform { get; set; }

        public Model Model { get; set; }

        public Material Material { get; set; }

        public void Dispose()
        {
            Model.Dispose();
        }
    }
}