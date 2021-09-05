using System;
using System.Numerics;

namespace Gerudo
{
    public class Camera
    {
        public Transform Transform { get; private set; }

        public float NearPlane { get => _nearPlane; set => _nearPlane = MathF.Max(0.01f, value); }

        public float FarPlane { get => _farPlane; set => _farPlane = MathF.Max(_nearPlane + 0.01f, value); }

        public float FieldOfView { get => _fov; set => _fov = MathHelper.Clamp(value, 0f, 180f); }

        private float _nearPlane;
        private float _farPlane;
        private float _fov;

        public Camera()
        {
            this.Transform = new Transform();
        }

        public CameraData GetCameraData()
        {
            return new CameraData()
            {
                viewMatrix = Matrix4x4.CreateLookAt(Transform.Position, Transform.Position - Transform.UnitZ, Transform.UnitY),
                projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView( MathHelper.DEG2RAD * FieldOfView, 16.0f / 9.0f, NearPlane, FarPlane)
            };
        }
    }

    public struct CameraData
    {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;
    }
}