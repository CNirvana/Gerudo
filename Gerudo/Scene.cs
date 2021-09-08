using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Gerudo
{
    public class Scene : IDisposable
    {
        public List<Renderer> Renderers { get; private set; } = new List<Renderer>();

        public Camera Camera { get; private set; }

        public DirectionalLight MainLight { get; private set; }

        public Scene()
        {
            Camera = new Camera()
            {
                NearPlane = 0.1f,
                FarPlane = 500.0f,
                FieldOfView = 60.0f,
            };
            Camera.Transform.Position = new System.Numerics.Vector3(0, 0, 10f);

            this.MainLight = new DirectionalLight()
            {
                LightColor = RgbaFloat.White,
                Intensity = 1f,
                Direction = Vector3.Normalize(Vector3.One)
            };
        }

        public void AddRenderer(Renderer renderer)
        {
            Renderers.Add(renderer);
        }

        public void Dispose()
        {
            foreach (var renderer in Renderers)
            {
                renderer.Dispose();
            }
            Renderers.Clear();
        }
    }
}